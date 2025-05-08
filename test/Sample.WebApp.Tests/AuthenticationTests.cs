using C3D.Extensions.Playwright.AspNetCore;
using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using System.Net;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class PlaywrightAuthenticationFixture : PlaywrightFixture<Program>
{
    public PlaywrightAuthenticationFixture(IMessageSink output) : base(output) { }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.AddBasicAuthentication();
        return base.CreateHost(builder);
    }
}

public class AuthenticationTests : IClassFixture<PlaywrightAuthenticationFixture>
{
    private readonly PlaywrightFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public AuthenticationTests(PlaywrightAuthenticationFixture webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

    [Fact]
    public async Task AnonymousCantAccessAdmin()
    {
        WriteFunctionName();

        await using var context = await webApplication.CreatePlaywrightContextPageAsync();
        var page = context.Page;

        // N.B. These tests work differently if the browser is not headless!
        IResponse? response = await page.GotoAsync("/Admin");

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.Unauthorized, response.Status);
    }

    [Fact]
    public async Task NonAdminCantAdmin()
    {
        WriteFunctionName();

        await using var context = await webApplication.CreateAuthorisedPlaywrightContextPageAsync("user");
        var page = context.Page;

        IResponse? response = await page.GotoAsync("/Admin");

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.Forbidden, response.Status);
    }

    [Fact]
    public async Task AdminCanAdmin()
    {
        WriteFunctionName();

        await using var context = await webApplication.CreateAuthorisedPlaywrightContextPageAsync(Security.Role.Admin);
        var page = context.Page;

        await page.GotoAsync("/Admin");

        Assert.Equal("Administration", await page.TitleAsync());
        Assert.Equal(webApplication.Uri + "/Admin", page.Url);
    }

    [Fact]
    public async Task AnonymousClientCantAccessAdmin()
    {
        WriteFunctionName();

        using var client = webApplication.CreateClient();

        var response = await client.GetAsync("/Admin");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NonAdminClientCantAdmin()
    {
        WriteFunctionName();

        using var client = webApplication.CreateClient("user");

        var response = await client.GetAsync("/Admin");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminClientCanAdmin()
    {
        WriteFunctionName();

        using var client = webApplication.CreateClient(Security.Role.Admin);

        var response = await client.GetAsync("/Admin");
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ct = response.Content.Headers.ContentType;
        Assert.NotNull(ct);
        Assert.Equal("text/html", ct.MediaType);
        Assert.Equal("utf-8", ct.CharSet);

        var body = await response.Content.ReadAsStringAsync();
        Assert.NotNull(body);
        Assert.Contains("<title>Administration</title>", body);
        
    }
}
