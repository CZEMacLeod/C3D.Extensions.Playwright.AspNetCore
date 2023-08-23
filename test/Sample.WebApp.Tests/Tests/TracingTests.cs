using C3D.Extensions.Playwright.AspNetCore.Utilities;
using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Playwright;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class TracingTests : IClassFixture<PlaywrightFixture<Program>>
{
    private readonly PlaywrightFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public TracingTests(PlaywrightFixture <Program> webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

    [Fact]
    public async Task CanTrace()
    {
        WriteFunctionName();

        const string ExpectedFileName = "CanTrace.zip";

        if (System.IO.File.Exists(ExpectedFileName))
        {
            System.IO.File.Delete(ExpectedFileName);
        }

        var page = await webApplication.CreatePlaywrightPageAsync();
        await using (var trace = await page.TraceAsync($"Testing Tracing on {webApplication.BrowserType}", true, true, true))
        {
            outputHelper.WriteLine($"Tracing to {trace.TraceName}");

            Assert.Equal(ExpectedFileName, trace.TraceName);

            await page.GotoAsync("/");

            Assert.Equal("Home page", await page.TitleAsync());

        }

        Assert.True(System.IO.File.Exists(ExpectedFileName));

        System.IO.File.Delete(ExpectedFileName);

        await page.CloseAsync();
    }

    [Fact(Skip = "Is interactive")]
    //[Fact]
    public async Task ShowTrace()
    {
        WriteFunctionName();

        const string ExpectedFileName = "ShowTrace.zip";

        if (System.IO.File.Exists(ExpectedFileName))
        {
            System.IO.File.Delete(ExpectedFileName);
        }

        var page = await webApplication.CreatePlaywrightPageAsync();
        await using (var trace = await page.TraceAsync($"Testing Tracing on {webApplication.BrowserType}", true, true, true))
        {
            outputHelper.WriteLine($"Tracing to {trace.TraceName}");
            trace.Show = PlaywrightTraceShow.OnClose; // Don't do this on a build server

            Assert.Equal(ExpectedFileName, trace.TraceName);

            await page.GotoAsync("/");

            Assert.Equal("Home page", await page.TitleAsync());
        }

        await page.CloseAsync(); ;
    }

    [Fact(Skip = "Is interactive")]
    //[Fact]
    public async Task ShowTraceAndWait()
    {
        WriteFunctionName();

        const string ExpectedFileName = "ShowTraceAndWait.zip";

        if (System.IO.File.Exists(ExpectedFileName))
        {
            System.IO.File.Delete(ExpectedFileName);
        }

        var page = await webApplication.CreatePlaywrightPageAsync();
        await using (var trace = await page.TraceAsync($"Testing Tracing on {webApplication.BrowserType}", true, true, true))
        {
            outputHelper.WriteLine($"Tracing to {trace.TraceName}");
            trace.Show = PlaywrightTraceShow.OnCloseAndWait; // Don't do this on a build server

            Assert.Equal(ExpectedFileName, trace.TraceName);

            await page.GotoAsync("/");

            Assert.Equal("Home page", await page.TitleAsync());
        }

        await page.CloseAsync(); ;
    }
}