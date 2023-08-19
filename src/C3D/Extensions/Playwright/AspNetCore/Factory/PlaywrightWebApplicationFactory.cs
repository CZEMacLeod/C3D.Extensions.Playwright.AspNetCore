using C3D.Extensions.Playwright.AspNetCore.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Diagnostics.CodeAnalysis;

namespace C3D.Extensions.Playwright.AspNetCore;

public class PlaywrightWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private IPlaywright? playwright;
    private IBrowser? browser;
    private IHostApplicationLifetime? lifetime;
    private CancellationTokenSource hostStarted = new();

    private string? uri;
    private int? port;
    // We randomize the server port so we ensure that any hard coded Uri's fail in the tests.
    // This also allows multiple servers to run during the tests.
    public int Port => port ??= 5000 + Interlocked.Add(ref nextPort, 10 + System.Random.Shared.Next(10));
    public string Uri => uri ??= $"http://localhost:{Port}";

    private static int nextPort = 0;

    #region "Overridable Properties"
    // Properties in this region can be overridden in a derived type and used as a fixture
    // If you create multiple derived fixtures, and derived tests injecting each one into a base test class
    // you can easily setup a test matrix for running a set of tests against multiple browsers and/or environments
    public virtual string? Environment { get; }
    public virtual PlaywrightBrowserType BrowserType => PlaywrightBrowserType.Chromium;

    protected virtual BrowserTypeLaunchOptions LaunchOptions { get; } = new()
    {
        Headless = true
    };

    private BrowserNewPageOptions? pageOptions;
    protected virtual BrowserNewPageOptions PageOptions => pageOptions ??= new()
    {
        BaseURL = Uri
    };

    private BrowserNewContextOptions? contextOptions;
    protected virtual BrowserNewContextOptions ContextOptions => contextOptions ??= new()
    {
        BaseURL = Uri
    };

    public virtual LogLevel MinimumLogLevel => LogLevel.Trace;
    #endregion

    protected virtual IBrowserType GetBrowser(PlaywrightBrowserType? browserType = null) => (browserType ?? BrowserType) switch
    {
        PlaywrightBrowserType.Chromium => playwright?.Chromium,
        PlaywrightBrowserType.Firefox => playwright?.Firefox,
        PlaywrightBrowserType.Webkit => playwright?.Webkit,
        _ => throw new ArgumentOutOfRangeException(nameof(browserType))
    } ?? throw new InvalidOperationException("Could not get browser type");

    protected virtual ILoggingBuilder ConfigureLogging(ILoggingBuilder builder)
    {
        builder.SetMinimumLevel(MinimumLogLevel);
        return builder;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (Environment is not null)
        {
            builder.UseEnvironment(Environment);
        }
        builder.ConfigureLogging(logging => ConfigureLogging(logging));

        // We the testHost, which can be used with HttpClient with a custom transport
        // It is assumed that the return of CreateHost is a host based on the TestHost Server.
        var testHost = base.CreateHost(builder);

        // Now we reconfigure the builder to use kestrel so we have an http listener that can be used by playwright
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel(options =>
        {
            options.ListenLocalhost(Port);
        }));
        var host = base.CreateHost(builder);

        lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() => hostStarted.Cancel());

        return new CompositeHost(testHost, host);
    }

    public async Task<IBrowser> GetDefaultPlaywrightBrowserAsync()
    {
        await EnsureServerStartedAsync(); // Ensure Server is initialized
        await InitializeAsync();          // Ensure Playwright is initialized

        return browser;
    }

    private async Task EnsureServerStartedAsync()
    {
        if (hostStarted.IsCancellationRequested) return;
        _ = Server;                 // Ensure Server is initialized
        await Task.Delay(Timeout.Infinite, hostStarted.Token).ContinueWith(tsk => { });
    }

    /// <summary>
    /// Creates a new Browser instance.
    /// </summary>
    /// <param name="browserType">Can be set to override the default BrowserType property</param>
    /// <param name="browserOptions">A callback that can be used to override the default LaunchOptions property</param>
    /// <returns>An IBrowser object</returns>
    /// <remarks> The consumer must ensure that the browser instance is closed and disposed of correctly.</remarks>
    public async Task<IBrowser> CreateCustomPlaywrightBrowserAsync(PlaywrightBrowserType? browserType = null,
                                                                   Action<BrowserTypeLaunchOptions>? browserOptions = null)
    {
        await EnsureServerStartedAsync(); // Ensure Server is initialized
        await InitializeAsync();          // Ensure Playwright is initialized
        EnsureBrowserInstalled(browserType);
        var launchOptions = new BrowserTypeLaunchOptions(LaunchOptions);
        browserOptions?.Invoke(launchOptions);
        return await LaunchAsync(GetBrowser(browserType), launchOptions);
    }

    private void EnsureBrowserInstalled(PlaywrightBrowserType? browserType)
    {
        PlaywrightUtilities.InstallPlaywright(browserType ?? BrowserType);
    }

    /// <summary>
    /// Creates a new Page instance using the web application URL and any custom options.
    /// </summary>
    /// <param name="pageOptions">A callback that can be used to override the default PageOptions property</param>
    /// <returns>An IPage object</returns>
    /// <remarks>The consumer should close the page when they are finished with it.</remarks>
    public async Task<IPage> CreatePlaywrightPageAsync(Action<BrowserNewPageOptions>? pageOptions = null)
    {
        await EnsureServerStartedAsync(); // Ensure Server is initialized
        await InitializeAsync();          // Ensure Playwright is initialized

        var options = new BrowserNewPageOptions(PageOptions);
        pageOptions?.Invoke(options);
        return await browser.NewPageAsync(options);
    }

    /// <summary>
    /// Creates a new Browser instance and a Page inside it, using the web application URL and any custom options.
    /// </summary>
    /// <param name="browserType">Can be set to override the default BrowserType property</param>
    /// <param name="browserOptions">A callback that can be used to override the default LaunchOptions property</param>
    /// <param name="pageOptions">A callback that can be used to override the default PageOptions property</param>
    /// <returns>A PlaywrightBrowserPage object which will correctly close the browser and page when disposed</returns>
    /// <remarks>The PlaywrightBrowserPage should be disposed of when finished with</remarks>
    public async Task<PlaywrightBrowserPage> CreateCustomPlaywrightBrowserPageAsync(PlaywrightBrowserType? browserType = null,
                                                                                    Action<BrowserTypeLaunchOptions>? browserOptions = null,
                                                                                    Action<BrowserNewPageOptions>? pageOptions = null)
    {
        var browser = await CreateCustomPlaywrightBrowserAsync(browserType, browserOptions);
        var options = new BrowserNewPageOptions(PageOptions);
        pageOptions?.Invoke(options);
        var page = await browser.NewPageAsync(options);
        return new(browser, page);
    }

    /// <summary>
    /// Creates a new Browser Context instance using the web application URL and any custom options.
    /// </summary>
    /// <param name="contextOptions">A callback that can be used to override the default ContextOptions property</param>
    /// <returns>An IBrowserContext object</returns>
    /// <remarks>The consumer is responsible for the correct closure and disposal of the context and any pages creates in it.</remarks>
    public async Task<IBrowserContext> CreatePlaywrightContextAsync(Action<BrowserNewContextOptions>? contextOptions = null)
    {
        await EnsureServerStartedAsync(); // Ensure Server is initialized
        await InitializeAsync();          // Ensure Playwright is initialized

        var options = new BrowserNewContextOptions(ContextOptions);
        contextOptions?.Invoke(options);
        return await browser.NewContextAsync(options);
    }

    /// <summary>
    /// Creates a new Browser Context instance and a Page inside it, using the web application URL and any custom options.
    /// </summary>
    /// <param name="contextOptions">A callback that can be used to override the default ContextOptions property</param>
    /// <param name="pageOptions">A callback that can be used to override the default PageOptions property</param>
    /// <returns>A PlaywrightContextPage object which will correctly close the browser and page when disposed</returns>
    /// <remarks>The PlaywrightContextPage should be disposed of when finished with</remarks>
    public async Task<PlaywrightContextPage> CreatePlaywrightContextPageAsync(Action<BrowserNewContextOptions>? contextOptions = null)
    {
        var context = await CreatePlaywrightContextAsync(contextOptions);
        var page = await context.NewPageAsync();
        return new(context, page);
    }


    [MemberNotNull(nameof(playwright), nameof(browser))]
    public virtual async Task InitializeAsync()
    {
        if (playwright is not null && browser is not null) return;

        PlaywrightUtilities.InstallPlaywright(BrowserType);
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        playwright ??= (await Microsoft.Playwright.Playwright.CreateAsync()) ?? throw new InvalidOperationException();
        browser ??= (await LaunchAsync(GetBrowser(), LaunchOptions)) ?? throw new InvalidOperationException();
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
    }

    private async Task<IBrowser> LaunchAsync(IBrowserType browser, BrowserTypeLaunchOptions options)
    {
        var args = (options.Args ?? Array.Empty<string>()).ToList();
        var ports = args.SingleOrDefault(arg => arg.StartsWith("--explicitly-allowed-ports"));
        if (ports is null && browser.Name == "chromium")
        {
            // In chromium some ports are blocked, such as sip ports 5060/5061.
            // As these may be picked, we explicitly allow whatever port this host is running on.
            args.Add($"--explicitly-allowed-ports={Port}");
        }
        options.Args = args;
        return await browser.LaunchAsync(options);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        playwright?.Dispose();
        playwright = null;
    }

    private bool isDisposed;
    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Dealt with by base class")]
    public async override ValueTask DisposeAsync()
    {
        if (browser is not null)
        {
            await browser.DisposeAsync();
        }
        browser = null;

        await base.DisposeAsync();

        isDisposed = true;
    }

    protected bool IsDisposed => isDisposed;

    public override string ToString() => $"{Environment}_{BrowserType}_{typeof(TProgram).FullName}";

    // CompositeHost is based on https://github.com/xaviersolau/DevArticles/blob/e2e_test_blazor_with_playwright/MyBlazorApp/MyAppTests/WebTestingHostFactory.cs
    // Relay the call to both test host and kestrel host.
    internal sealed class CompositeHost : IHost, IServiceProvider
    {
        private readonly IHost testHost;
        private readonly IHost kestrelHost;
        public CompositeHost(IHost testHost, IHost kestrelHost)
        {
            this.testHost = testHost;
            this.kestrelHost = kestrelHost;
        }
        public IServiceProvider Services => this;
        public void Dispose()
        {
            testHost.Dispose();
            kestrelHost.Dispose();
            GC.SuppressFinalize(this);
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IEnumerable<IHost>)) return new IHost[] { testHost, kestrelHost };
            if (serviceType == typeof(IEnumerable<IServer>)) return new IServer[] { 
                testHost.Services.GetRequiredService<IServer>(), 
                kestrelHost.Services.GetRequiredService<IServer>()
            };
            return testHost.Services.GetService(serviceType);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await testHost.StartAsync(cancellationToken);
            await kestrelHost.StartAsync(cancellationToken);
        }
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await testHost.StopAsync(cancellationToken);
            await kestrelHost.StopAsync(cancellationToken);
        }
    }
}
