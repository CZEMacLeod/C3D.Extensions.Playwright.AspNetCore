# C3D.Extensions.Playwright.AspNetCore.Authentication

An extension to `Microsoft.AspNetCore.Mvc.Testing` and `C3D.Extensions.Playwright.AspNetCore` which adds authentication support to the `WebApplicationFactory`.

This allows you to write Playwright browser based tests that use and test authentication.

The authentication uses the [`idunno.Authentication.Basic`](https://github.com/blowdart/idunno.Authentication) package to provide 'Basic Authentication'. 
This should not (normally) be used in a production environement, but provides an easy to use mechansim to generate authentication tokens on the server side,
and matching credentials on the client side.

## Setup

When creating a 'test' host using `IHostBuilder`, you can use the `AddBasicAuthentication` extension method to enable the embedded `idunno.Authentication.Basic` authentication system.
```cs
	builder.AddBasicAuthentication();
```

This will generate a claims user when the username == the password.
The claims will include the username, displayname and role (which will all be the same).

You can add an optional function to add additional claims as a parameter to the `AddBasicAuthentication` call.
The function takes `ValidateCredentialsContext` and string parameters representing the context and the username/role (which are equal).
It is an async function that returns `Task<IEnumerable<Claim>?>`. This allows you to not return any additional claims.

There is an overload that takes the `TRole` type of the registered RoleManager from Microsoft.AspNetCore.Identity to lookup the role and add any role specific claims.
This can be called as

```cs
	builder.AddBasicAuthentication<AppRole>();
```

Obviously this is not secure in any way, and should only be used in a test scenario, e.g. during Playwright testing.

## Usage

When you have a host that is setup to support BasicAuthention, you can then create a Playwright browser context (effectively an in-private isolated session), which will include the appropriate authentication header.
There is an extension method to the `PlaywrightFixture<TProgram>` called `CreateAuthorisedPlaywrightContextPageAsync` which takes the rolename to use.
This creates a new context and page (which should be disposed at the end of the test), with a Basic Authentication header with the username and password equal to the passed in role.


### Sample

An example of using this with `XUnit` is available in the github repository.

```cs
public class PlaywrightAuthenticationFixture : PlaywrightFixture<Program>
{
    public PlaywrightAuthenticationFixture(IMessageSink output) : base(output) { }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.AddBasicAuthentication();
        return base.CreateHost(builder);
    }
}
```

```cs
public class AuthenticationTests : IClassFixture<PlaywrightAuthenticationFixture>
{
    private readonly PlaywrightFixture<Program> webApplication;

    public AuthenticationTests(PlaywrightAuthenticationFixture webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
    }

    [Fact]
    public async Task RandomTest()
    {
        await using var context = await webApplication.CreateAuthorisedPlaywrightContextPageAsync("SomeRole");
        var page = context.Page;

        await page.GotoAsync("/Somewhere");
    }
}
```

## HttpClient

While this package is primarliy designed for use with Playwright, you may also require to use the `HttpClient` features of `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory`.

This package provides a number of overloads of the basic `CreateClient` method on `WebApplicationFactory<TProgram>`.
The first takes a function to configure a `WebApplicationFactoryAuthenticatedClientOptions` which is an augmented version of `WebApplicationFactoryClientOptions` and defaults to (a copy of) the settings from `WebApplicationFactory<TProgram>.ClientOptions`.
The additional properties `UserName`, `Password` and `Handlers` are available. Setting either (or both) of the authentication properties results in an `AuthenticationHeaderValue` being added to each request made.

`Handlers` allows you to add additional middleware handlers into the configuration. 
This allows you to use the Authentication, Redirection, and Cookie handlers at the same time as custom ones without having to manually add them all.

There are 2 additional overloads of `CreateClient`, one which takes `username` and `password` as parameters, and another which takes a single string `role` which is used as both `username` and `password`.
These are syntactic sugar over the configuration method mentioned previously.

