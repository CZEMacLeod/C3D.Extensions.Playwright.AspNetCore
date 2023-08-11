# C3D.Extensions.Playwright.AspNetCore

An extension to `Microsoft.AspNetCore.Mvc.Testing` which adds `Microsoft.Playwright` support to the `WebApplicationFactory` (and keeps the existing HttpClient infrastucture).

This allows you to write browser based tests as well as API based tests using this as the bases for a test fixture.

There are also a number of utility methods available to install playwrite and to help with tracing.

## Factory

The main factory is `PlaywrightWebApplicationFactory` which can be used in place of `WebApplicationFactory`.
You can derive your own type from this factory, with a concrete type and override the various members to allow customizing the instance.

### Extension Points

```cs
    public override string? Environment => "Staging";
    public override PlaywrightBrowserType BrowserType => PlaywrightBrowserType.Firefox;
    public override LogLevel MinimumLogLevel => LogLevel.Debug;
```
These can be used to adjust the hosting environment, browser, and log level.

### Usage

The factory adds a `CreatePlaywrightPageAsync` method to compliment the `CreateClient` method.
This returns an `IPage` from playwright which allows you to do your browser testing.
The underlying engine will spin up an instance of your website on a random port above 5000, and set the base url for playwright so you can use relative URLs for GotoPageAsync.
Remember the factory is IAsyncDisposable so you should dispose it when finished with.

```cs
    using var webApplication = new PlaywrightWebApplicationFactory<Program>();
    var page = await webApplication.CreatePlaywrightPageAsync();

    await page.GotoAsync("/");
    var title = await page.TitleAsync();
```

## Extensions

There are a number of overloads of an extension method `TraceAsync` added to `IPage` to allow easier creation of traces.
The only required parameter is the title for the trace.
The filename is generated from the `CallerMemberName`. It appends .zip and optionally adds the prefix parameter (with an _ to seperate).

If you use one of the overloads that takes a type
```cs
.TraceAsync<MyType>("title")
.TraceAsync("title",typeof(MyType))
```
it will use the type's FullName as the prefix.

There is another overload that will ToString on an object passed in to get the prefix.
The factory implements 
```cs
public override string ToString() => $"{Environment}_{BrowserType}_{typeof(TProgram).FullName}";
```

The object returned is `IAsyncDisposable` and will close the trace when disposed.

```cs
    using var webApplication = new PlaywrightWebApplicationFactory<Program>();
    var page = await webApplication.CreatePlaywrightPageAsync();
    using var trace = await page.TraceAsync("MyTrace");

    await page.GotoAsync("/");
    var title = await page.TitleAsync();
```

The trace object has a public `TraceName` property with the built path, and a `ShowOnClose` property which,
if set to true, will attempt to open the trace file on dispose.

## Utilities

The `PlaywrightUtilities` class provides a couple of methods to make recording traces easier.

The `ShowTrace` method takes a filename and optional parameters which are passed to the trace.

Note that the filename must include the .zip extension.