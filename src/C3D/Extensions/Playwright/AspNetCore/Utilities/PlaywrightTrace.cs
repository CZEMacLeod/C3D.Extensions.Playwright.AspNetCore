using Microsoft.Playwright;
using System.Diagnostics.CodeAnalysis;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightTrace : IAsyncDisposable
{
    internal PlaywrightTrace(IPage page)
    {
        this.page = page;
    }
    private readonly IPage page;

    private string? path;

    public string? TraceName => path;

    public bool ShowOnClose { get; set; }

    internal async Task InitializeAsync(TracingStartOptions options)
    {
        await page.Context.Tracing.StartAsync(options);
        path = options.Name;
    }

    public async ValueTask DisposeAsync()
    {
        await page.Context.Tracing.StopAsync(new()
        {
            Path = path
        });
        if (path is not null && ShowOnClose)
        {
            await PlaywrightUtilities.ShowTraceAsync(path);
        }
        GC.SuppressFinalize(this);
    }
}
