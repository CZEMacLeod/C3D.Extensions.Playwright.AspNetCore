using idunno.Authentication.Basic;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Microsoft.Extensions.Hosting;

public static class HostBuilderBasicAuthenticationExtensions
{ 
	/// <summary>
	/// Registers a basic authentication scheme that succeeds for password==username and assigns the role of the username
	/// </summary>
	public static IHostBuilder AddBasicAuthentication(this IHostBuilder builder,
        Func<ValidateCredentialsContext, string, Task<IEnumerable<Claim>?>>? roleClaimsFunc = null) =>
		builder.ConfigureServices(services => services.AddBasicAuthentication(roleClaimsFunc));

	/// <summary>
	/// Uses a registered RoleManager from Microsoft.AspNetCore.Identity to lookup the role and add any role specific claims.
	/// </summary>
	/// <typeparam name="TRole">Class used for the Role</typeparam>
	/// <param name="services">The main service collection</param>
	/// <returns></returns>
	public static IHostBuilder AddBasicAuthentication<TRole>(this IHostBuilder builder)
		where TRole : class => builder.ConfigureServices(services => services.AddBasicAuthentication<TRole>());
}
