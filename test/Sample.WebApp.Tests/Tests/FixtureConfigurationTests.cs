using C3D.Extensions.Playwright.AspNetCore;
using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Sample.WebApp.Tests.Fixtures;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

#region Fixtures
public class SlowMoFixtureOverrideOptions : ConfigurationTestFixture
{
	public SlowMoFixtureOverrideOptions(IMessageSink output) : base(output) { }

	protected override Microsoft.Playwright.BrowserTypeLaunchOptions LaunchOptions
	{
		get
		{
			var options = base.LaunchOptions;
			options.Headless = true;	// If headless is set to false in config it tends to upset the speed tests
			options.SlowMo = 100;		// Slow down operations by 100ms
			return options;
		}
	}
}

public class SlowMoFixtureAddConfiguration : ConfigurationTestFixture
{
	public SlowMoFixtureAddConfiguration(IMessageSink output) : base(output) { }

	protected override IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder config) =>
		base.ConfigureConfiguration(config)
			.AddInMemoryCollection(new Dictionary<string, string>()
					{
						{ TestConfigurationSection + ":LaunchOptions:Headless", "true" }, // If headless is set to false in config it tends to upset the speed tests
						{ TestConfigurationSection + ":LaunchOptions:SlowMo", "100" }
					});
}
#endregion

[Trait("Category", "FixtureConfigurationTest")]
public abstract class FixtureConfigurationTestBase
{
	private readonly PlaywrightFixture<Program> webApplication;
	private IPlaywrightFixtureOptions? PlaywrightOptions => webApplication as IPlaywrightFixtureOptions;
	private readonly ITestOutputHelper outputHelper;

	public FixtureConfigurationTestBase(PlaywrightFixture<Program> webApplication, ITestOutputHelper outputHelper)
	{
		this.webApplication = webApplication;
		this.outputHelper = outputHelper;
	}

	private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

	[Fact]
	public void CheckDefaultBrowserLaunchConfigurationIsSlowMo()
	{
		WriteFunctionName();

		Assert.NotNull(PlaywrightOptions);
		Assert.Equal(100, PlaywrightOptions.LaunchOptions.SlowMo);

	}

	[Fact]
	public async Task CheckLaunchedBrowserUsesConfiguration()
	{
		WriteFunctionName();

		await using var sloMoPage = await webApplication.CreatePlaywrightDisposablePageAsync();
		var normalPage = await webApplication.CreateCustomPlaywrightBrowserPageAsync(browserOptions: options =>
		{
			options.SlowMo = 0;
		});

		var normalTime = await PerformTimedOperationsOnPage(normalPage.Page);
		var sloMoTime = await PerformTimedOperationsOnPage(sloMoPage.Page);
		outputHelper.WriteLine("Normal Time: {0}ms", normalTime.TotalMilliseconds);
		outputHelper.WriteLine("SloMo Time: {0}ms", sloMoTime.TotalMilliseconds);
		outputHelper.WriteLine("Time Difference: {0}ms", (sloMoTime - normalTime).TotalMilliseconds);

		Assert.True(sloMoTime > normalTime);
	}

	private static async Task<TimeSpan> PerformTimedOperationsOnPage(IPage page)
	{
		var start = System.Diagnostics.Stopwatch.GetTimestamp();

		await page.GotoAsync("/");
		var navItems = page.Locator("li.nav-item");

		Assert.NotNull(navItems);

		Assert.Equal(2, await navItems.CountAsync());

		var link = navItems.Nth(1);

		Assert.NotNull(link);

		await link.ClickAsync();
		await page.WaitForLoadStateAsync();
		Assert.Equal("Privacy Policy", await page.TitleAsync());

		return TimeSpan.FromTicks(System.Diagnostics.Stopwatch.GetTimestamp() - start);

	}
}

// Perform base tests using SlowMoFixtureOverrideOptions
public class FixtureConfigurationOverrideOptionsTest : FixtureConfigurationTestBase, IClassFixture<SlowMoFixtureOverrideOptions>
{
	public FixtureConfigurationOverrideOptionsTest(SlowMoFixtureOverrideOptions webApplication, ITestOutputHelper outputHelper) : base(webApplication, outputHelper)
	{
	}
}

// Perform base tests using SlowMoFixtureAddConfiguration
public class FixtureConfigurationAddConfigurationTest : FixtureConfigurationTestBase, IClassFixture<SlowMoFixtureAddConfiguration>
{
	public FixtureConfigurationAddConfigurationTest(SlowMoFixtureAddConfiguration webApplication, ITestOutputHelper outputHelper) : base(webApplication, outputHelper)
	{
	}
}