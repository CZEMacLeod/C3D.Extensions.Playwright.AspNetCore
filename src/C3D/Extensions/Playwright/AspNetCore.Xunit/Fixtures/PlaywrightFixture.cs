using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace C3D.Extensions.Playwright.AspNetCore.Xunit;

public class PlaywrightFixture<TProgram> : PlaywrightWebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private readonly IMessageSink output;
    public PlaywrightFixture(IMessageSink output) => this.output = output;

    public virtual bool AddMessageSinkProvider => true;

    public IMessageSink MessageSink => output;

    protected override ILoggingBuilder ConfigureLogging(ILoggingBuilder builder)
    {
        if (AddMessageSinkProvider) builder.AddXunit(output);
        return base.ConfigureLogging(builder);
    }

    #region "IAsyncLifetime"
    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
    #endregion
}
