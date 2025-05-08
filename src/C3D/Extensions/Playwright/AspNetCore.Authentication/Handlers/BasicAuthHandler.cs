namespace C3D.Extensions.Playwright.AspNetCore.Authentication.Handlers;

public class BasicAuthHandler : DelegatingHandler
{
    private readonly string? username;
    private readonly string? password;

    public BasicAuthHandler(string? username, string? password)
    {
        this.username = username;
        this.password = password;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = BasicAuthHeaderUtilities.BasicAuthHeader(username, password);
        return base.SendAsync(request, cancellationToken);
    }
}