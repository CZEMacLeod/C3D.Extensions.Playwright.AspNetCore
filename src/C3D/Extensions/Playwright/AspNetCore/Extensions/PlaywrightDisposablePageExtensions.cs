using C3D.Extensions.Playwright.AspNetCore.Utilities;
using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace C3D.Extensions.Playwright.AspNetCore;

public static class PlaywrightDisposablePageExtensions
{
    public static Task<PlaywrightTrace> TraceAsync<T>(this IPlaywrightDisposablePage page, string title, [CallerMemberName] string? name = null,
    Action<TracingStartOptions>? options = null)
    => page.TraceAsync(title, typeof(T), name, options);

    public static Task<PlaywrightTrace> TraceAsync(this IPlaywrightDisposablePage page, string title, Type type,
        [CallerMemberName] string? name = null,
        Action<TracingStartOptions>? options = null)
        => page.TraceAsync(title, type.FullName, name, options);

    public static Task<PlaywrightTrace> TraceAsync<T>(this IPlaywrightDisposablePage page, string title, T prefix, [CallerMemberName] string? name = null,
        Action<TracingStartOptions>? options = null)
        where T : class
        => page.TraceAsync(title, prefix.ToString(), name, options);
}
