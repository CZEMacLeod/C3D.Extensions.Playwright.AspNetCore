using Microsoft.Playwright;
using Xunit.Abstractions;

namespace C3D.Extensions.Playwright.AspNetCore.Xunit.Fixtures;

public class PlaywrightPageFixture<TProgram> : PlaywrightFixture<TProgram>
    where TProgram : class
{
    private IPage? page;

    public PlaywrightPageFixture(IMessageSink output) : base(output) { }

    public IPage? Page => page ?? throw (IsDisposed ? new ObjectDisposedException(nameof(Page)) :  new Exception("Not initialized"));

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();
        page = await CreatePlaywrightPageAsync();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Base class calls SuppressFinalize")]
    public override async ValueTask DisposeAsync()
    {
        if (page is not null)
        {
            await page.CloseAsync();
        }
        page = null;

        await base.DisposeAsync();
    }
}
