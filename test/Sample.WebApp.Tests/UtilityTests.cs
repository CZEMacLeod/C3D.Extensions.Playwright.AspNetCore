using C3D.Extensions.Playwright.AspNetCore;
using C3D.Extensions.Playwright.AspNetCore.Xunit;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class UtilityTests : IClassFixture<PlaywrightFixture<Program>>
{
    private readonly PlaywrightFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public UtilityTests(PlaywrightFixture<Program> webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

    [Fact]
    public async Task CheckHomePageTitleInNewBrowser()
    {
        WriteFunctionName();

        await using var browserPage = await webApplication.CreateCustomPlaywrightBrowserPageAsync();
        var page = browserPage.Page;    // This is for convienence only
        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());
        // We don't need to close the page here as the browserPage utility object will do it for us.
    }

    [Fact]
    public async Task CheckHomePageTitleInFirefox()
    {
        WriteFunctionName();

        await using var browserPage = await webApplication.CreateCustomPlaywrightBrowserPageAsync(PlaywrightBrowserType.Firefox);
        var page = browserPage.Page;    // This is for convienence only
        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());
        // We don't need to close the page here as the browserPage utility object will do it for us.
    }

    [Fact]
    public async Task CheckHomePageTitleInWebkit()
    {
        WriteFunctionName();

        await using var browserPage = await webApplication.CreateCustomPlaywrightBrowserPageAsync(PlaywrightBrowserType.Webkit);
        var page = browserPage.Page;    // This is for convienence only
        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());
        // We don't need to close the page here as the browserPage utility object will do it for us.
    }

    [Fact]
    public async Task CheckHomePageTitleIncognito()
    {
        WriteFunctionName();

        await using var incognitoPage = await webApplication.CreatePlaywrightContextPageAsync();
        var page = incognitoPage.Page;    // This is for convienence only
        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());
        // We don't need to close the page here as the browserPage utility object will do it for us.
    }

}