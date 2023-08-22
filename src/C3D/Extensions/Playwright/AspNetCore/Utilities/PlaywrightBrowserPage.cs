using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightBrowserPage : PlaywrightDisposablePage
{
    private IBrowser? browser;

    internal PlaywrightBrowserPage(IBrowser browser, IPage page, TracingStartOptions traceOptions) : 
        base(page, traceOptions) => this.browser = browser;

    public IBrowser Browser => browser ?? throw new ObjectDisposedException(nameof(Browser));

    protected async override Task OnDisposeAsync()
    {
        await base.OnDisposeAsync();

        if (browser is not null) await browser.CloseAsync();
        browser = null;
    }
}
