using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Sample.WebApp.Tests.Attributes;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

[TestCaseOrderer(
    ordererTypeName: "Sample.WebApp.Tests.Orderers.PriorityOrderer",
    ordererAssemblyName: "Sample.WebApp.Tests")]
public class PageTests : IClassFixture<PlaywrightPageFixture<Program>>
{
    private readonly PlaywrightPageFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public PageTests(PlaywrightPageFixture<Program> webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

    [Fact, TestPriority(1)]
    public async Task OpenHomePage()
    {
        WriteFunctionName();

        await webApplication.Page.GotoAsync("/");

        Assert.True(await webApplication.Page.IsVisibleAsync("body"));

    }

    [Fact, TestPriority(2)]
    public async Task CheckHomePageTitle()
    {
        WriteFunctionName();

        Assert.Equal("Home page", await webApplication.Page.TitleAsync());
    }

    [Fact, TestPriority(3)]
    public async Task NavigatePrivacyPage()
    {
        WriteFunctionName();

        var navItems = webApplication.Page.Locator("li.nav-item");

        Assert.NotNull(navItems);

        Assert.Equal(2, await navItems.CountAsync());

        var link = navItems.Nth(1);

        Assert.NotNull(link);

        await link.ClickAsync();
        await webApplication.Page.WaitForLoadStateAsync();
    }

    [Fact, TestPriority(4)]
    public async Task CheckPrivacyPageTitle()
    {
        WriteFunctionName();

        Assert.Equal("Privacy Policy", await webApplication.Page.TitleAsync());
    }
}
