# C3D.Extensions.Playwright.AspNetCore.Xunit

Adds Xunit logging and fixture support to `C3D.Extensions.Playwright.AspNetCore` to allow easy unit testing of AspNetCore web applications using `Xunit`.

## Usage

### Basic

Create an Xunit est with a class fixture. Use Playwright with tracing support to check a page title.

```cs
public class UnitTestDefault : IClassFixture<PlaywrightFixture<Program>>
{
    private readonly PlaywrightFixture<Program> webApplication;
    private readonly ITestOutputHelper outputHelper;

    public UnitTestDefault(PlaywrightFixture<Program> webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    [Fact]
    public async Task CanTrace()
    {
        var page = await webApplication.CreatePlaywrightPageAsync();
        await using var trace = await page.TraceAsync($"Testing Tracing on {webApplication.BrowserType}", true, true, true);

        outputHelper.WriteLine($"Tracing to {trace.TraceName}");

        Assert.Equal("CanTrace.zip", trace.TraceName);

        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());

        Assert.True(System.IO.File.Exists(trace.TraceName));
    }
}
```

### Advanced

Create a base fixture wrapping your program.
```cs
public abstract class ProgramFixtureBase : PlaywrightFixture<Program> {
    protected ProgramFixtureBase(IMessageSink output) : base(output) {}
}
```

Create multiple fixtures for each combination of Environment and/or browser that you desire.

```cs
public class ProgramFixtureChrome : ProgramFixtureBase {
    public ProgramFixtureChrome(IMessageSink output) : base(output) {}
}
public class ProgramFixtureFirefox : ProgramFixtureBase {
    public ProgramFixtureFirefox(IMessageSink output) : base(output) {}
    public override PlaywrightBrowserType BrowserType => PlaywrightBrowserType.Firefox;
}
public class ProgramFixtureWebkit : ProgramFixtureBase {
    public ProgramFixtureWebkit(IMessageSink output) : base(output) {}
    public override PlaywrightBrowserType BrowserType => PlaywrightBrowserType.Webkit;
}
```

Create a base unit test class

```cs
public abstract class UnitTestBase 
{
    protected readonly ProgramFixtureBase webApplication;
    protected readonly ITestOutputHelper outputHelper;

    protected UnitTestBase(ProgramFixtureBase webApplication, ITestOutputHelper outputHelper)
    {
        this.webApplication = webApplication;
        this.outputHelper = outputHelper;
    }

    [Fact]
    public async Task TestRootTitle()
    {
        var page = await webApplication.CreatePlaywrightPageAsync();

        await page.GotoAsync("/");
        Assert.Equal("Home page", await page.TitleAsync());
    }
}
```

Create a unit test class for each fixture

```cs
public class UnitTestChrome : UnitTestBase, IClassFixture<ProgramFixtureChrome>
{
    public UnitTestChrome(ProgramFixtureChrome webApplication, ITestOutputHelper outputHelper) : 
        base(webApplication, outputHelper)
    {
    }
}
public class UnitTestFirefox : UnitTestBase, IClassFixture<ProgramFixtureFirefox>
{
    public UnitTestFirefox(ProgramFixtureFirefox webApplication, ITestOutputHelper outputHelper) : 
        base(webApplication, outputHelper)
    {
    }
}
public class UnitTestWebkit : UnitTestBase, IClassFixture<ProgramFixtureWebkit>
{
    public UnitTestWebkit(ProgramFixtureWebkit webApplication, ITestOutputHelper outputHelper) : 
        base(webApplication, outputHelper)
    {
    }
}
```

Now each test in `UnitTestBase` will be run for each browser.

If using tracing, in order to prevent clashes between tests which will have the same name by default, use the overload taking the type of the fixture.

There is an overload that takes an object and calls ToString to get the prefix. 
Using this with the fixture class will return
```cs
{Environment}_{BrowserType}_{typeof(TProgram).FullName}
```
You can override this in your base fixture depending on what variants you have defined.