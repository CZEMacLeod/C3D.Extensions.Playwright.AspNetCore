using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task CheckServerUrls()
    {
        WriteFunctionName();

        var page = await webApplication.CreatePlaywrightPageAsync();
        await page.GotoAsync("/");

        var uri = page.Url;
        var webAppUri = new Uri(webApplication.Uri).ToString(); // Note, this will add the trailing / to the Uri

        var addresses = webApplication.Services.GetServices<IServer>()
            .SelectMany(server => server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Enumerable.Empty<string>());

        outputHelper.WriteLine("Playwright Url: {0}", uri);
        outputHelper.WriteLine("Web Application Url: {0}", webApplication.Uri);
        foreach (var address in addresses)
        {
            outputHelper.WriteLine("Server Address: {0}", address);
        }


        Assert.Equal(webAppUri, uri);                                               // Check playwright goes to expected page
        Assert.Collection(addresses, 
            address => Assert.Equal(address,webApplication.Uri)
        );     // Check the server listens only on the expected address

    }
}
