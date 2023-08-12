using Microsoft.Playwright;

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

    public PlaywrightTraceShow Show { get; set; }

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
