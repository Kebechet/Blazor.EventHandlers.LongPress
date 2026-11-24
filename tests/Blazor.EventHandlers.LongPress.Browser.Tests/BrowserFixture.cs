using Microsoft.Playwright;

namespace Blazor.EventHandlers.LongPress.Browser.Tests;

/// <summary>
/// One Chromium and one WebKit for a test class: Chromium is the engine of Android's WebView and most browsers,
/// WebKit the one of iOS.
/// </summary>
public sealed class BrowserFixture : IAsyncLifetime
{
    public const string Chromium = "chromium";
    public const string WebKit = "webkit";

    private readonly Dictionary<string, IBrowser> _browsersByName = [];
    private IPlaywright? _playwright;

    public async ValueTask InitializeAsync()
    {
        // Downloads the browser builds this Playwright version drives, once per machine; it returns at once when they
        // are already there.
        var installExitCode = Microsoft.Playwright.Program.Main(["install", Chromium, WebKit]);
        installExitCode.ShouldBe(0, "Playwright could not install its browsers");

        _playwright = await Playwright.CreateAsync();
        _browsersByName[Chromium] = await _playwright.Chromium.LaunchAsync();
        _browsersByName[WebKit] = await _playwright.Webkit.LaunchAsync();
    }

    public IBrowser GetBrowser(string browserName)
    {
        return _browsersByName[browserName];
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var browser in _browsersByName.Values)
        {
            await browser.CloseAsync();
        }

        _playwright?.Dispose();
    }
}
