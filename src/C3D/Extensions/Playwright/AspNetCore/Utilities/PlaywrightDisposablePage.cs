using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightDisposablePage : IPlaywrightDisposablePage
{
    private IPage? page;
    private readonly TracingStartOptions? traceOptions;

    internal PlaywrightDisposablePage(IPage page, TracingStartOptions? traceOptions)
    {
        this.page = page;
        this.traceOptions = traceOptions;
    }

    public virtual IBrowserContext Context => page?.Context ?? throw new ObjectDisposedException(nameof(Page));

    public IPage Page => page ?? throw new ObjectDisposedException(nameof(Page));

    private bool _disposed;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await OnDisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    protected virtual async Task OnDisposeAsync()
    {
        if (page is not null) await page.CloseAsync();
        page = null;
    }

    public async Task<PlaywrightTrace> TraceAsync(string title, string? prefix = null, [CallerMemberName] string? name = null,
        Action<TracingStartOptions>? options = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PlaywrightContextPage));

        var traceOptions = new TracingStartOptions(this.traceOptions!)
        {
            Name = prefix is null ? $"{name}.zip" : $"{prefix}_{name}.zip",
            Title = title
        };
        options?.Invoke(traceOptions);
        var trace = new PlaywrightTrace(Context);
        await trace.InitializeAsync(traceOptions);
        return trace;
    }
}
