using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightContextPage : PlaywrightDisposablePage
{
    private IBrowserContext? context;

    internal PlaywrightContextPage(IBrowserContext context, IPage page, TracingStartOptions traceOptions) : 
        base(page, traceOptions) => this.context = context;

    public override IBrowserContext Context => context ?? throw new ObjectDisposedException(nameof(Context));

    protected override async Task OnDisposeAsync()
    {
        await base.OnDisposeAsync();
        if (context is not null) await context.CloseAsync();
        context = null;
    }
}
