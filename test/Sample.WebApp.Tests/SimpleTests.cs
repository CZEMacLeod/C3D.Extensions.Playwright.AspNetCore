using C3D.Extensions.Playwright.AspNetCore.Xunit;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class SimpleTests : IClassFixture<PlaywrightFixture<Program>>
{
    private readonly PlaywrightFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public SimpleTests(PlaywrightFixture<Program> webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

    [Fact]
    public async Task CheckHomePageTitle()
    {
        WriteFunctionName();

        var page = await webApplication.CreatePlaywrightPageAsync();
        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());
        await page.CloseAsync();
    }

}