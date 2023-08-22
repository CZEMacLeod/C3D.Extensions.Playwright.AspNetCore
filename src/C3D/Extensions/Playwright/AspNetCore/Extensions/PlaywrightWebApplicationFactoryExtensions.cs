using Microsoft.Playwright;

namespace C3D.Extensions.Playwright.AspNetCore;

public static class PlaywrightWebApplicationFactoryExtensions
{
    /// <summary>
    /// Creates an IAsyncDisposable wrapper around the page which ensures it will be properly closed when the object is disposed.
    /// </summary>
    /// <param name="factory">A PlaywrightWebApplicationFactory</param>
    /// <param name="pageOptions">Custom configuration options for this page</param>
    /// <returns></returns>
    public static async Task<IPlaywrightDisposablePage> CreatePlaywrightDisposablePageAsync<TProgram>(
        this PlaywrightWebApplicationFactory<TProgram> factory,
        Action<BrowserNewPageOptions>? pageOptions = null) 
        where TProgram : class => (await factory.CreatePlaywrightPageAsync(pageOptions)).AsAsyncDisposable(factory);
}
