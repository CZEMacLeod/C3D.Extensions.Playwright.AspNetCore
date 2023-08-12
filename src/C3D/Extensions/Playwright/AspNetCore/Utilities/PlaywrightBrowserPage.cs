using Microsoft.Playwright;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightBrowserPage : IAsyncDisposable
{
    private IBrowser? browser;
    private IPage? page;

    internal PlaywrightBrowserPage(IBrowser browser, IPage page)
    {
        this.browser = browser;
        this.page = page;
    }

    public IBrowser Browser => browser ?? throw new ObjectDisposedException(nameof(Browser));

    public IPage Page => page ?? throw new ObjectDisposedException(nameof(Page));

    private bool _disposed;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (page is not null) await page.CloseAsync();
        page = null;

        if (browser is not null) await browser.CloseAsync();
        browser = null;

        _disposed=true;
        GC.SuppressFinalize(this);
    }
}
