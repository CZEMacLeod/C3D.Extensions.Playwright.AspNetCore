using Microsoft.Playwright;

namespace Sample.WebApp.Tests.Fixtures;

public interface IPlaywrightFixtureOptions
{
    BrowserTypeLaunchOptions LaunchOptions { get; }
    BrowserNewContextOptions ContextOptions { get; }
    BrowserNewPageOptions PageOptions { get; }
    TracingStartOptions TracingOptions { get; }
}
