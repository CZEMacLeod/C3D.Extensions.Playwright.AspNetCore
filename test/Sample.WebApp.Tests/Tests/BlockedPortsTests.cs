using C3D.Extensions.Playwright.AspNetCore;
using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class Port5060Fixture : PlaywrightFixture<Program>
{
    public Port5060Fixture(IMessageSink output) : base(output) { }

    public override int Port => 5060;   // Force the application to port 5060.
}

public class BlockedPortsTests : IClassFixture<Port5060Fixture>
{
    private readonly PlaywrightFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public BlockedPortsTests(Port5060Fixture webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);


    [Fact]
    public async Task ChromiumAllowsBlockedPort()
    {
        WriteFunctionName();

        var page = await webApplication.CreatePlaywrightPageAsync();
        await page.GotoAsync("/");

        var pageTitle = await page.TitleAsync();
        var uri = page.Url;
        var webAppUri = new Uri(webApplication.Uri).ToString(); // Note, this will add the trailing / to the Uri

        await page.CloseAsync();

        Assert.Equal(webAppUri, uri);               // Check browser goes to expected page
        Assert.Equal("Home page", pageTitle);       // Check browser can read title
    }


    [Fact]
    public async Task FireFoxAllowsBlockedPort()
    {
        WriteFunctionName();

        await using var browserPage = await webApplication.CreateCustomPlaywrightBrowserPageAsync(PlaywrightBrowserType.Firefox);
        var page = browserPage.Page;    // This is for convienence only
        await page.GotoAsync("/");

        var pageTitle = await page.TitleAsync();
        var uri = page.Url;
        var webAppUri = new Uri(webApplication.Uri).ToString(); // Note, this will add the trailing / to the Uri
        // We don't need to close the page here as the browserPage utility object will do it for us.

        Assert.Equal(webAppUri, uri);               // Check browser goes to expected page
        Assert.Equal("Home page", pageTitle);       // Check browser can read title
    }

    [Fact]
    public async Task WebkitDoesNotAllowBlockedPort()
    {
        WriteFunctionName();

        await using var browserPage = await webApplication.CreateCustomPlaywrightBrowserPageAsync(PlaywrightBrowserType.Webkit);
        var page = browserPage.Page;    // This is for convienence only
        await page.GotoAsync("/");

        var pageTitle = await page.TitleAsync();
        var uri = page.Url;
        var webAppUri = new Uri(webApplication.Uri).ToString(); // Note, this will add the trailing / to the Uri
        // We don't need to close the page here as the browserPage utility object will do it for us.

        Assert.NotEqual(webAppUri, uri);            // Check browser *didn't* go to expected page
        Assert.NotEqual("Home page", pageTitle);    // Check browser *didn't* read title
    }

}
