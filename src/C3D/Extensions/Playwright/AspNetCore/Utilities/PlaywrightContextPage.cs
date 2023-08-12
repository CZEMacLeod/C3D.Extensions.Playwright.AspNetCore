using Microsoft.Playwright;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightContextPage : IAsyncDisposable
{
    private IBrowserContext? context;
    private IPage? page;

    internal PlaywrightContextPage(IBrowserContext context, IPage page)
    {
        this.context = context;
        this.page = page;
    }

    public IBrowserContext Context => context ?? throw new ObjectDisposedException(nameof(Context));

    public IPage Page => page ?? throw new ObjectDisposedException(nameof(Page));

    private bool _disposed;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (page is not null) await page.CloseAsync();
        page = null;

        if (context is not null) await context.CloseAsync();
        context = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
