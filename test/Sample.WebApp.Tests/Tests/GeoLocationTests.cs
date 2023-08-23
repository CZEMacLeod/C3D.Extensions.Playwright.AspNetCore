using C3D.Extensions.Playwright.AspNetCore;
using C3D.Extensions.Playwright.AspNetCore.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Sample.WebApp.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class GeoLocationFixture : ConfigurationTestFixture
{
	public GeoLocationFixture(IMessageSink output) : base(output) { }

	protected override IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder config) =>
		base.ConfigureConfiguration(config)
			.AddInMemoryCollection(new Dictionary<string, string>()
				{
					{ TestConfigurationSection + ":PageOptions:Permissions:0", "geolocation" }
				});
}

public class GeoLocationTests : IClassFixture<GeoLocationFixture>
{
	private readonly PlaywrightFixture<Program> webApplication;
	private readonly ITestOutputHelper outputHelper;

	public GeoLocationTests(GeoLocationFixture webApplication, ITestOutputHelper outputHelper)
	{
		this.webApplication = webApplication;
		this.outputHelper = outputHelper;
	}

	private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

	[Fact]
	public async Task GetGeoLocationReturnsValues()
	{
		WriteFunctionName();

		await using var pageWrapper = await webApplication.CreatePlaywrightDisposablePageAsync(options =>
		{
			options.Geolocation = new Geolocation()
			{
				Latitude = 57,
				Longitude = 2
			};
		});
		var page = pageWrapper.Page;

		await page.GotoAsync("/GeoLocation");
		await page.ClickAsync("#getLocationButton");

		var locationSpans = await page.Locator("span.location").AllAsync();
		var locationTasks = locationSpans.Select(async location=> new { 
			id = await location.GetAttributeAsync("id"), 
			value = await location.InnerTextAsync() 
		});
		var locations = await Task.WhenAll(locationTasks);

		Assert.Collection(locations, 
			location =>
			{
				Assert.Equal("latitude", location.id);
				Assert.Equal("57", location.value);
			},
			location =>
			{
				Assert.Equal("longitude", location.id);
				Assert.Equal("2", location.value);
			});
	}
}
