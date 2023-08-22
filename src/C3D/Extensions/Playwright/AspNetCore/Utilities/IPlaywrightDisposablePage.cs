using C3D.Extensions.Playwright.AspNetCore.Utilities;
using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace C3D.Extensions.Playwright.AspNetCore
{
    public interface IPlaywrightDisposablePage : IAsyncDisposable
    {
        IPage Page { get; }

        Task<PlaywrightTrace> TraceAsync(string title, string? prefix = null, [CallerMemberName] string? name = null, Action<TracingStartOptions>? options = null);
    }
}