using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication;

public static class CredentialValidationExtensions {
	public static Claim DefaultRoleClaim<TOptions>(this ResultContext<TOptions> context, string roleName)
		where TOptions : AuthenticationSchemeOptions
		=> new(ClaimTypes.Role,
			   roleName,
			   ClaimValueTypes.String,
			   context.Options.ClaimsIssuer);
}