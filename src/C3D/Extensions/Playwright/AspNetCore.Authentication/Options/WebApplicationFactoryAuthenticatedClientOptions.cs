using C3D.Extensions.Playwright.AspNetCore.Authentication.Handlers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace C3D.Extensions.Playwright.AspNetCore.Authentication.Options;

public class WebApplicationFactoryAuthenticatedClientOptions : WebApplicationFactoryClientOptions
{
    public WebApplicationFactoryAuthenticatedClientOptions()
    {
    }

    // Copy constructor
    internal WebApplicationFactoryAuthenticatedClientOptions(WebApplicationFactoryClientOptions clientOptions)
    {
        BaseAddress = clientOptions.BaseAddress;
        AllowAutoRedirect = clientOptions.AllowAutoRedirect;
        MaxAutomaticRedirections = clientOptions.MaxAutomaticRedirections;
        HandleCookies = clientOptions.HandleCookies;

        if (clientOptions is WebApplicationFactoryAuthenticatedClientOptions authOptions)
        {
            UserName = authOptions.UserName;
            Password = authOptions.Password;
            Handlers = authOptions.Handlers;
        }
    }

    public string? UserName { get; set; }
    public string? Password { get; set; }

    public IEnumerable<DelegatingHandler> Handlers { get; set; } = Enumerable.Empty<DelegatingHandler>();

    internal protected virtual DelegatingHandler[] CreateHandlers()
    {
        return CreateHandlersCore().Concat(Handlers).ToArray();

        IEnumerable<DelegatingHandler> CreateHandlersCore()
        {
            if (!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
            {
                yield return new BasicAuthHandler(UserName, Password);
            }
            if (AllowAutoRedirect)
            {
                yield return new RedirectHandler(MaxAutomaticRedirections);
            }
            if (HandleCookies)
            {
                yield return new CookieContainerHandler();
            }
        }
    }
}
