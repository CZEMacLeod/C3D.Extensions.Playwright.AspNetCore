using C3D.Extensions.Playwright.AspNetCore;
using C3D.Extensions.Playwright.AspNetCore.Utilities;
using System.Runtime.CompilerServices;

namespace Microsoft.Playwright;

public static class PlaywrightPageExtensions
{
    public static IPlaywrightDisposablePage AsAsyncDisposable<TProgram>(this IPage page, PlaywrightWebApplicationFactory<TProgram> factory)
        where TProgram : class => page.AsAsyncDisposable(factory.TracingOptions);

    public static IPlaywrightDisposablePage AsAsyncDisposable(this IPage page, TracingStartOptions? traceOptions = null) =>
        new PlaywrightDisposablePage(page, traceOptions);

    // TODO: Should we mark this overload as obsolete too?
    public static async Task<PlaywrightTrace> TraceAsync(this IPage page,
        string title, bool? screenshots = null, bool? snapshots = null, bool? sources = null,
        string? prefix = null,
        [CallerMemberName] string? name = null,
        TracingStartOptions? defaults = null) => await page.TraceAsync(title, prefix, name, defaults, options =>
        {
            if (screenshots is not null) options.Screenshots = screenshots;
            if (snapshots is not null) options.Snapshots = snapshots;
            if (sources is not null) options.Sources = sources;
        });

    [Obsolete("Use overload with options configuration action and set Screenshots, Snapshots, and Sources as required.")]
    public static Task<PlaywrightTrace> TraceAsync<T>(this IPage page, string title,
        bool? screenshots = null, bool? snapshots = null, bool? sources = null,
        [CallerMemberName] string? name = null)
        => page.TraceAsync(title, typeof(T), screenshots, snapshots, sources, name);

    [Obsolete("Use overload with options configuration action and set Screenshots, Snapshots, and Sources as required.")]
    public static Task<PlaywrightTrace> TraceAsync(this IPage page, string title, Type type,
        bool? screenshots = null, bool? snapshots = null, bool? sources = null,
        [CallerMemberName] string? name = null)
        => page.TraceAsync(title, screenshots, snapshots, sources, type.FullName, name);

    [Obsolete("Use overload with options configuration action and set Screenshots, Snapshots, and Sources as required.")]
    public static Task<PlaywrightTrace> TraceAsync<T>(this IPage page, string title, T prefix,
        bool? screenshots = null, bool? snapshots = null, bool? sources = null,
        [CallerMemberName] string? name = null)
        where T : class
        => page.TraceAsync(title, screenshots, snapshots, sources, prefix.ToString(), name);

    public static async Task<PlaywrightTrace> TraceAsync(this IPage page,
        string title, string? prefix = null, [CallerMemberName] string? name = null,
        TracingStartOptions? defaults = null,
        Action<TracingStartOptions>? options = null)
    {
        var traceOptions = new TracingStartOptions(defaults!)
        {
            Name = prefix is null ? $"{name}.zip" : $"{prefix}_{name}.zip",
            Title = title
        };
        options?.Invoke(traceOptions);
        var trace = new PlaywrightTrace(page);
        await trace.InitializeAsync(traceOptions);
        return trace;
    }

    public static Task<PlaywrightTrace> TraceAsync<T>(this IPage page, string title, [CallerMemberName] string? name = null,
        TracingStartOptions? defaults = null,
        Action<TracingStartOptions>? options = null)
    => page.TraceAsync(title, typeof(T), name, defaults, options);

    public static Task<PlaywrightTrace> TraceAsync(this IPage page, string title, Type type,
        [CallerMemberName] string? name = null,
        TracingStartOptions? defaults = null,
        Action<TracingStartOptions>? options = null)
        => page.TraceAsync(title, type.FullName, name, defaults, options);

    public static Task<PlaywrightTrace> TraceAsync<T>(this IPage page, string title, T prefix, [CallerMemberName] string? name = null,
        TracingStartOptions? defaults = null,
        Action<TracingStartOptions>? options = null)
        where T : class
        => page.TraceAsync(title, prefix.ToString(), name, defaults, options);
}

