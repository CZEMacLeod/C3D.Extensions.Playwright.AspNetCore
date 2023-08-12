namespace C3D.Extensions.Playwright.AspNetCore.Utilities;

public class PlaywrightUtilities
{

    private static readonly object installLock = new();
    private static readonly List<PlaywrightBrowserType> installed = new();
    /// <summary>
    /// Install and deploy all binaries Playwright may need.
    /// </summary>
    public static void InstallPlaywright(PlaywrightBrowserType? browser = null)
    {
        // Lock here so we don't try installing from multiple fixtures.
        lock (installLock)
        {
            if (browser is null)
            {
                if (installed.Contains(PlaywrightBrowserType.Chromium) &&
                    installed.Contains(PlaywrightBrowserType.Firefox) &&
                    installed.Contains(PlaywrightBrowserType.Webkit))
                {
                    return;
                }
            }
            else if (installed.Contains(browser.Value))
            {
                return;
            }
            var parameters = new List<string>()
            {
                "install"
            };
            if (browser is not null)
            {
                parameters.Add(browser.Value.ToString().ToLowerInvariant());
            }
            parameters.Add("--with-deps");
            var exitCode = Microsoft.Playwright.Program.Main(parameters.ToArray());
            if (exitCode != 0)
            {
                throw new Exception(
                  $"Playwright exited with code {exitCode} on {string.Join(' ', parameters)}");
            }
            if (browser is null)
            {
                installed.Add(PlaywrightBrowserType.Chromium);
                installed.Add(PlaywrightBrowserType.Firefox);
                installed.Add(PlaywrightBrowserType.Webkit);
            }
            else
            {
                installed.Add(browser.Value);
            }
        }
    }

    public static void UninstallPlaywright(PlaywrightBrowserType? browser = null)
    {
        lock (installLock)
        {
            var args = new[] { "uninstall", browser is null ? "--all" : browser.Value.ToString().ToLowerInvariant() };
            var exitCode = Microsoft.Playwright.Program.Main(args);
            if (exitCode != 0)
            {
                throw new Exception(
                  $"Playwright exited with code {exitCode} on {string.Join(' ', args)}");
            }
            if (browser is null)
            {
                installed.Clear();
            }
            else
            {
                installed.Remove(browser.Value);
            }

        }
    }

    public static void ShowTrace(string traceFile, params string[] options)
    {
        var args = new List<string>() { "show-trace" };
        args.AddRange(options);
        args.Add(traceFile);
        var exitCode = Microsoft.Playwright.Program.Main(args.ToArray());
        if (exitCode != 0)
        {
            throw new Exception(
              $"Playwright exited with code {exitCode} on show-trace");
        }
    }

    public static Task ShowTraceAsync(string traceFile, params string[] options)
    {
        return Task.Run(() =>
        {
            var args = new List<string>() { "show-trace" };
            args.AddRange(options);
            args.Add(traceFile);
            var exitCode = Microsoft.Playwright.Program.Main(args.ToArray());
            if (exitCode != 0)
            {
                throw new Exception(
                  $"Playwright exited with code {exitCode} on show-trace");
            }
        });
    }
}
