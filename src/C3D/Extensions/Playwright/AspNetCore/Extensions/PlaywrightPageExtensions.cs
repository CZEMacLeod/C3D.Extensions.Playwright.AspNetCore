using System.Runtime.CompilerServices;
using C3D.Extensions.Playwright.AspNetCore.Utilities;

namespace Microsoft.Playwright;

public static class PlaywrightPageExtensions
{
    public static async Task<PlaywrightTrace> TraceAsync(this IPage page, 
        string title, bool? screenshots=null, bool? snapshots=null, bool? sources=null, 
        string? prefix = null,
        [CallerMemberName] string? name = null)
    {
        var trace = new PlaywrightTrace(page);
        await trace.InitializeAsync(new()
        {
            Screenshots = screenshots,
            Snapshots = snapshots,
            Sources = sources,
            Name = prefix is null ? $"{name}.zip" : $"{prefix}_{name}.zip",
            Title = title
        });
        return trace;
    }

    public static Task<PlaywrightTrace> TraceAsync<T>(this IPage page, string title, bool? screenshots = null, bool? snapshots = null, bool? sources = null, [CallerMemberName] string? name = null)
        => page.TraceAsync(title, typeof(T), screenshots, snapshots, sources, name);

    public static Task<PlaywrightTrace> TraceAsync(this IPage page, string title, Type type, bool? screenshots = null, bool? snapshots = null, bool? sources = null, [CallerMemberName] string? name = null)
        => page.TraceAsync(title, screenshots, snapshots, sources, type.FullName, name);

    public static Task<PlaywrightTrace> TraceAsync<T>(this IPage page, string title, T prefix, bool? screenshots = null, bool? snapshots = null, bool? sources = null, [CallerMemberName] string? name = null)
        where T : class
        => page.TraceAsync(title, screenshots, snapshots, sources, prefix.ToString(), name);
}
