using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests.Fixtures;

public abstract class ConfigurationTestFixture : PlaywrightFixture<Program>, IPlaywrightFixtureOptions
{
    protected ConfigurationTestFixture(IMessageSink output) : base(output)    {    }

    BrowserTypeLaunchOptions IPlaywrightFixtureOptions.LaunchOptions => base.LaunchOptions;
    BrowserNewContextOptions IPlaywrightFixtureOptions.ContextOptions => base.ContextOptions;
    BrowserNewPageOptions IPlaywrightFixtureOptions.PageOptions => base.PageOptions;
    TracingStartOptions IPlaywrightFixtureOptions.TracingOptions => base.TracingOptions;
}
