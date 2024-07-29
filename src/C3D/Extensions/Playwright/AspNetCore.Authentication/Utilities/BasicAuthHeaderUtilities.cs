using idunno.Authentication.Basic;
using System.Net.Http.Headers;
using System.Text;

namespace C3D.Extensions.Playwright.AspNetCore.Authentication;

public static class BasicAuthHeaderUtilities
{
    public static AuthenticationHeaderValue BasicAuthHeader(string? username, string? password)
    {
        var rawUserPassword = Encoding.UTF8.GetBytes($"{username ?? string.Empty}:{password ?? string.Empty}");
        var base64UserPassword = Convert.ToBase64String(rawUserPassword);
        return new AuthenticationHeaderValue(scheme: BasicAuthenticationDefaults.AuthenticationScheme, base64UserPassword);
    }

    private const string Authorization = "Authorization";
    private static KeyValuePair<string, string> BasicAuthHeaderPair(string? username, string? password) =>
        new(Authorization, BasicAuthHeader(username, password).ToString());

    public static IEnumerable<KeyValuePair<string, string>> BasicAuthHeaders(IEnumerable<KeyValuePair<string, string>> headers, 
        string username, string password)
        => headers
            .Where(x => x.Key != Authorization)
            .Append(BasicAuthHeaderPair(username, password));

    public static IEnumerable<KeyValuePair<string, string>> BasicAuthHeaders(string username, string password)
    {
        yield return BasicAuthHeaderPair(username, password);
    }
}
