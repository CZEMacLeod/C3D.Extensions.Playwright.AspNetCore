using C3D.Extensions.Playwright.AspNetCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sample.WebApp.Tests;

public class InstallerTests
{
    private readonly ITestOutputHelper outputHelper;

    public InstallerTests(ITestOutputHelper outputHelper) => this.outputHelper = outputHelper;

    private void WriteFunctionName([CallerMemberName] string? caller = null) => outputHelper.WriteLine(caller);

    [Fact]
    public void InstallTest()
    {
        WriteFunctionName();

        PlaywrightUtilities.InstallPlaywright();

        string packagePath = GetBinariesPath();

        Assert.NotEqual(string.Empty, packagePath);

        outputHelper.WriteLine($"Path: {packagePath}");

        packagePath = Environment.ExpandEnvironmentVariables(packagePath);
        
        Assert.NotEqual(string.Empty, packagePath);

        var folders = GetDirectories(packagePath);

        foreach (var folder in folders) { outputHelper.WriteLine($"Folder: {folder}"); }

        Assert.Contains(folders, f => f.StartsWith("chromium-"));
        Assert.Contains(folders, f => f.StartsWith("firefox-"));
        Assert.Contains(folders, f => f.StartsWith("webkit-"));
    }

    private static string GetBinariesPath()
    {
        var custom = Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH");
        if (!string.IsNullOrEmpty(custom)) return custom;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "%USERPROFILE%\\AppData\\Local\\ms-playwright";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "~/Library/Caches/ms-playwright";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "~/.cache/ms-playwright";
        return string.Empty;
    }

    [Fact(Skip = "Do not run the uninstall when other tests might be running")]
    //[Fact]
    public void UninstallTest()
    {
        WriteFunctionName();

        PlaywrightUtilities.UninstallPlaywright();

        string packagePath = GetBinariesPath();

        Assert.NotEqual(string.Empty, packagePath);

        packagePath = Environment.ExpandEnvironmentVariables(packagePath);

        outputHelper.WriteLine($"Path: {packagePath}");

        Assert.NotEqual(string.Empty, packagePath);

        var folders = GetDirectories(packagePath);

        foreach (var folder in folders) { outputHelper.WriteLine($"Folder: {folder}"); }

        Assert.DoesNotContain(folders, f => f.StartsWith("chromium-"));
        Assert.DoesNotContain(folders, f => f.StartsWith("firefox-"));
        Assert.DoesNotContain(folders, f => f.StartsWith("webkit-"));
    }

    private static IEnumerable<string> GetDirectories(string packagePath)
    {
        return System.IO.Directory.GetDirectories(packagePath).Select(path => new DirectoryInfo(path).Name);
    }
}
