using Microsoft.Playwright;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightTrace : IAsyncDisposable
{

    internal PlaywrightTrace(IPage page) : this(page.Context) { }

    internal PlaywrightTrace(IBrowserContext context) => this.context = context;

    private string? path;
    private readonly IBrowserContext context;

    public string? TraceName => path;

    public PlaywrightTraceShow Show { get; set; }

    internal async Task InitializeAsync(TracingStartOptions options)
    {
        await context.Tracing.StartAsync(options);
        path = options.Name;
    }

    public async ValueTask DisposeAsync()
    {
        await context.Tracing.StopAsync(new()
        {
            Path = path
        });
        if (path is not null && Show != PlaywrightTraceShow.None)
        {
            if (Show == PlaywrightTraceShow.OnCloseAndWait)
            {
                await PlaywrightUtilities.ShowTraceAsync(path);
            }
            else
            {
                _ = PlaywrightUtilities.ShowTraceAsync(path);   // FAF
            }
        }
        GC.SuppressFinalize(this);
    }
}
