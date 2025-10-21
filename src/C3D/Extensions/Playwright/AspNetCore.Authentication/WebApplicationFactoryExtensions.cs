using C3D.Extensions.Playwright.AspNetCore.Authentication;
using C3D.Extensions.Playwright.AspNetCore.Authentication.Options;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using System.Reflection.PortableExecutable;

namespace C3D.Extensions.Playwright.AspNetCore;

public static class WebApplicationFactoryExtensions
{
    public static BrowserNewContextOptions WithBasicAuthentication(this BrowserNewContextOptions options, string username, string password)
    {
        options.ExtraHTTPHeaders = options.ExtraHTTPHeaders is null
            ? BasicAuthHeaderUtilities.BasicAuthHeaders(username, password)
            : BasicAuthHeaderUtilities.BasicAuthHeaders(options.ExtraHTTPHeaders, username, password);
        return options;
    }

    public static async Task<Utilities.PlaywrightContextPage> CreateAuthorisedPlaywrightContextPageAsync<TProgram>(
        this PlaywrightWebApplicationFactory<TProgram> fixture, string username, string password, Action<BrowserNewContextOptions>? contextOptions = null)
        where TProgram : class =>
        await fixture.CreatePlaywrightContextPageAsync(contextOptions: options => {
            contextOptions?.Invoke(options);
            options.WithBasicAuthentication(username,password);
        });

    public static Task<Utilities.PlaywrightContextPage> CreateAuthorisedPlaywrightContextPageAsync<TProgram>(
        this PlaywrightWebApplicationFactory<TProgram> fixture, string role, Action<BrowserNewContextOptions>? contextOptions = null)
        where TProgram : class => fixture.CreateAuthorisedPlaywrightContextPageAsync(role, role, contextOptions);

    public static HttpClient CreateClient<TProgram>(this WebApplicationFactory<TProgram> fixture, Action<WebApplicationFactoryAuthenticatedClientOptions> options)
        where TProgram : class
    {
        var clientOptions = new WebApplicationFactoryAuthenticatedClientOptions(fixture.ClientOptions);
        options?.Invoke(clientOptions);
        return fixture.CreateDefaultClient(clientOptions.BaseAddress, clientOptions.CreateHandlers());
    }

    public static HttpClient CreateClient<TProgram>(this WebApplicationFactory<TProgram> fixture, string username, string password)
        where TProgram : class => fixture.CreateClient(options =>
                                       {
                                           options.UserName = username;
                                           options.Password = password;
                                       });

    public static HttpClient CreateClient<TProgram>(this WebApplicationFactory<TProgram> fixture, string role)
        where TProgram : class => fixture.CreateClient(role, role);
}
