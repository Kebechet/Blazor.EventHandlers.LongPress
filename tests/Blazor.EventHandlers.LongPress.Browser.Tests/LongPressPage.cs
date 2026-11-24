using System.Diagnostics;
using Microsoft.Playwright;

namespace Blazor.EventHandlers.LongPress.Browser.Tests;

/// <summary>
/// A page with one button and the shipping long-press-event.min.js, which counts the longpress and click events the
/// button receives.
/// </summary>
internal sealed class LongPressPage : IAsyncDisposable
{
    private const string Origin = "http://longpress.test";
    private const string ScriptFileName = "long-press-event.min.js";
    private const string ButtonSelector = "#target";

    private static string _scriptPath => Path.Combine(AppContext.BaseDirectory, "BrowserAssets", ScriptFileName);

    private readonly IBrowserContext _browserContext;
    private readonly IPage _page;
    private readonly Stopwatch _pressStopwatch = new();

    private LongPressPage(IBrowserContext browserContext, IPage page)
    {
        _browserContext = browserContext;
        _page = page;
    }

    /// <param name="shouldPreventDefault">
    /// Whether the longpress listener calls preventDefault, which tells the script to swallow the click that follows.
    /// </param>
    public static async Task<LongPressPage> Open(IBrowser browser, bool shouldPreventDefault = false)
    {
        var pageHtml = $$"""
            <!doctype html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="margin:0">
            <button id="target" style="position:absolute;left:100px;top:100px;width:200px;height:100px">Hold me</button>
            <script>
                window.longPressCount = 0;
                window.clickCount = 0;
                const target = document.getElementById('target');
                target.addEventListener('longpress', e => {
                    window.longPressCount++;
                    if ({{(shouldPreventDefault ? "true" : "false")}}) {
                        e.preventDefault();
                    }
                });
                target.addEventListener('click', () => window.clickCount++);
            </script>
            <script src="/{{ScriptFileName}}"></script>
            </body>
            </html>
            """;

        var browserContext = await browser.NewContextAsync();
        var page = await browserContext.NewPageAsync();
        await page.RouteAsync($"{Origin}/**", async route =>
        {
            var isScriptRequest = route.Request.Url.EndsWith(ScriptFileName);
            await route.FulfillAsync(new RouteFulfillOptions
            {
                ContentType = isScriptRequest
                    ? "text/javascript"
                    : "text/html",
                Body = isScriptRequest
                    ? await File.ReadAllTextAsync(_scriptPath)
                    : pageHtml,
            });
        });
        await page.GotoAsync($"{Origin}/");

        return new LongPressPage(browserContext, page);
    }

    public async Task<int> GetLongPressCount()
    {
        return await _page.EvaluateAsync<int>("() => window.longPressCount");
    }

    public async Task<int> GetClickCount()
    {
        return await _page.EvaluateAsync<int>("() => window.clickCount");
    }

    /// <summary>The button's centre in CSS pixels.</summary>
    private async Task<(float X, float Y)> GetButtonCentre()
    {
        var box = await _page.Locator(ButtonSelector).BoundingBoxAsync();
        box.ShouldNotBeNull();

        return (box.X + (box.Width / 2), box.Y + (box.Height / 2));
    }

    public async Task MoveToButton()
    {
        var (x, y) = await GetButtonCentre();
        await _page.Mouse.MoveAsync(x, y);
    }

    /// <summary>Presses the mouse where it is, through the browser's real input pipeline, and starts timing the press.</summary>
    public async Task Press()
    {
        _pressStopwatch.Restart();
        await _page.Mouse.DownAsync();
    }

    public async Task MoveBy(int distancePixels)
    {
        var (x, y) = await GetButtonCentre();
        await _page.Mouse.MoveAsync(x + distancePixels, y);
    }

    /// <summary>Returns once the mouse has been held for <paramref name="holdDuration"/> since <see cref="Press"/>.</summary>
    public async Task WaitUntilHeldFor(TimeSpan holdDuration)
    {
        var remainingDuration = holdDuration - _pressStopwatch.Elapsed;
        if (remainingDuration > TimeSpan.Zero)
        {
            await Task.Delay(remainingDuration);
        }
    }

    public async Task Release()
    {
        await _page.Mouse.UpAsync();
    }

    /// <summary>Releases the mouse once it has been held for <paramref name="holdDuration"/> since <see cref="Press"/>.</summary>
    public async Task ReleaseAfter(TimeSpan holdDuration)
    {
        await WaitUntilHeldFor(holdDuration);
        await Release();
    }

    /// <summary>
    /// Keeps the page's main thread busy for <paramref name="duration"/>, starting right after this returns, the way a
    /// render or a garbage collection in a WebAssembly app does. Input that arrives meanwhile waits in the queue.
    /// </summary>
    public async Task BlockMainThread(TimeSpan duration)
    {
        await _page.EvaluateAsync(
            "durationMilliseconds => setTimeout(() => { const end = performance.now() + durationMilliseconds; while (performance.now() < end) { } }, 0)",
            duration.TotalMilliseconds);
    }

    public async ValueTask DisposeAsync()
    {
        await _browserContext.CloseAsync();
    }
}
