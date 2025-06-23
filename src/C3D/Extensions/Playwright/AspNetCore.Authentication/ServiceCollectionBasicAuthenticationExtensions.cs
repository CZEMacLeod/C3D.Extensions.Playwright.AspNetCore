using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionBasicAuthenticationExtensions
{
    /// <summary>
    /// Registers a basic authentication scheme that succeeds for password==username and assigns the role of the username
    /// </summary>
    public static IServiceCollection AddBasicAuthentication(this IServiceCollection services,
        Func<ValidateCredentialsContext, string, Task<IEnumerable<Claim>?>>? roleClaimsFunc = null)
        => services
                .AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasic(options =>
                {
                    options.Realm = "Test Realm";
                    options.AllowInsecureProtocol = true;
                    options.Events = new BasicAuthenticationEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger<BasicAuthenticationEvents>();
                            logger.LogError(context.Exception, "Authentication failed");
                            return Task.CompletedTask;
                        },
                        OnValidateCredentials = async context =>
                        {
                            if (context.Username == context.Password)
                            {

                                var userClaims = new[]
                                    {
										// Set UserName
										new Claim(
                                                ClaimTypes.NameIdentifier,
                                                context.Username,
                                                ClaimValueTypes.String,
                                                context.Options.ClaimsIssuer),
										// Set DisplayName
										new Claim(
                                                ClaimTypes.Name,
                                                context.Username,
                                                ClaimValueTypes.String,
                                                context.Options.ClaimsIssuer)
                                    };


                                var roleClaims = roleClaimsFunc is null ?
                                    Enumerable.Repeat(context.DefaultRoleClaim(context.Username), 1) :
                                    (await roleClaimsFunc.Invoke(context, context.Username) ?? Enumerable.Empty<Claim>());

                                context.Principal = new ClaimsPrincipal(
                                        new ClaimsIdentity(userClaims.Concat(roleClaims), context.Scheme.Name));
                                context.Success();
                            }
                        }
                    };
                })
                .Services;

    /// <summary>
    /// Uses a registered RoleManager from Microsoft.AspNetCore.Identity to lookup the role and add any role specific claims.
    /// </summary>
    /// <typeparam name="TRole">Class used for the Role</typeparam>
    /// <param name="services">The main service collection</param>
    /// <returns></returns>
    public static IServiceCollection AddBasicAuthentication<TRole>(this IServiceCollection services)
        where TRole : class => services.AddBasicAuthentication(async (context, roleName) =>
                                    {
                                        // This bit is probably overkill for most testing needs.
                                        // Simply adding the role, regardless of whether it exists, to the claim is enough for most scenarios.
                                        // But, in case there is anything custom added to the Role under RoleManager, we lookup the role and any custom claims.
                                        var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                                        var role = await roleManager.FindByNameAsync(roleName);
                                        IList<Claim> roleClaims = (role is not null ? await roleManager.GetClaimsAsync(role) : null) ?? Enumerable.Empty<Claim>().ToList();
                                        if (role is not null)
                                        {
                                            roleClaims.Add(context.DefaultRoleClaim(roleName));
                                        }
                                        return roleClaims;
                                    });

}
