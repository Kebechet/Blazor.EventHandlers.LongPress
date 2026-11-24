namespace Blazor.EventHandlers.LongPress.Browser.Tests;

public class LongPressBrowserTests(BrowserFixture browserFixture) : IClassFixture<BrowserFixture>
{
    /// <summary>Well past the package's default 400ms delay.</summary>
    private static readonly TimeSpan _longHold = TimeSpan.FromMilliseconds(600);

    /// <summary>Well inside the 400ms delay.</summary>
    private static readonly TimeSpan _quickTap = TimeSpan.FromMilliseconds(150);

    /// <summary>Long enough for a timer that should not fire to have fired.</summary>
    private static readonly TimeSpan _settleDuration = TimeSpan.FromMilliseconds(600);

    /// <summary>Lets the busy loop start before the mouse is pressed or released, so that input waits behind it.</summary>
    private static readonly TimeSpan _busyLoopStartDelay = TimeSpan.FromMilliseconds(50);

    /// <summary>How long the main thread is busy at the touch, as a render or a GC pause in a WebAssembly app can be.</summary>
    private static readonly TimeSpan _busyAtTouchDuration = TimeSpan.FromMilliseconds(350);

    /// <summary>
    /// Past the 400ms delay, yet released before the script's timer, which only starts once the busy main thread runs
    /// the pointerdown handler, could reach it.
    /// </summary>
    private static readonly TimeSpan _holdPastTheDelayWithALateTimer = TimeSpan.FromMilliseconds(500);

    /// <summary>Starts before the release below and ends after it, so the pointerup waits behind it.</summary>
    private static readonly TimeSpan _busyAcrossTheReleaseDuration = TimeSpan.FromMilliseconds(600);

    /// <summary>Well past the 400ms delay, released while the main thread is still busy.</summary>
    private static readonly TimeSpan _holdReleasedWhileBusy = TimeSpan.FromMilliseconds(500);

    /// <summary>Why a longpress counted before the release would make a release test prove nothing.</summary>
    private const string RaisedBeforeTheReleaseMessage =
        "The timer raised the longpress before the release, so the release path was never exercised.";

    /// <summary>
    /// The event is renamed from long-press to longpress, which Blazor can bind, and the default delay lowered from 1500ms
    /// to Android's 400ms.
    /// </summary>
    [Theory]
    [InlineData(BrowserFixture.Chromium)]
    [InlineData(BrowserFixture.WebKit)]
    public async Task Hold_PastTheDefaultDelay_RaisesLongPressBeforeTheRelease(string browserName)
    {
        // Arrange
        await using var page = await LongPressPage.Open(browserFixture.GetBrowser(browserName));
        await page.MoveToButton();

        // Act
        await page.Press();
        await Task.Delay(_longHold);
        var longPressCountWhileHeld = await page.GetLongPressCount();
        await page.ReleaseAfter(_longHold);

        // Assert
        longPressCountWhileHeld.ShouldBe(1);
        var longPressCount = await page.GetLongPressCount();
        longPressCount.ShouldBe(1);
    }

    [Theory]
    [InlineData(BrowserFixture.Chromium)]
    [InlineData(BrowserFixture.WebKit)]
    public async Task Tap_ShorterThanTheDelay_RaisesNoLongPress(string browserName)
    {
        // Arrange
        await using var page = await LongPressPage.Open(browserFixture.GetBrowser(browserName));
        await page.MoveToButton();

        // Act
        await page.Press();
        await page.ReleaseAfter(_quickTap);
        await Task.Delay(_settleDuration);

        // Assert
        var longPressCount = await page.GetLongPressCount();
        longPressCount.ShouldBe(0);
        var clickCount = await page.GetClickCount();
        clickCount.ShouldBe(1);
    }

    [Theory]
    [InlineData(BrowserFixture.Chromium)]
    [InlineData(BrowserFixture.WebKit)]
    public async Task Hold_AfterMovingAway_RaisesNoLongPress(string browserName)
    {
        // Arrange
        await using var page = await LongPressPage.Open(browserFixture.GetBrowser(browserName));
        await page.MoveToButton();

        // Act
        await page.Press();
        await page.MoveBy(40);
        await page.ReleaseAfter(_longHold);
        await Task.Delay(_settleDuration);

        // Assert
        var longPressCount = await page.GetLongPressCount();
        longPressCount.ShouldBe(0);
    }

    /// <summary>
    /// The patch from https://github.com/john-doherty/long-press-event/pull/57. The script's timer starts when its
    /// pointerdown handler runs and checks once per animation frame, and its pointerup used to cancel it, so a press held
    /// past the delay but released before the timer caught up raised nothing. The release now decides from the events'
    /// own timestamps. This fails if an update of the script drops the patch.
    /// </summary>
    [Theory]
    [InlineData(BrowserFixture.Chromium)]
    [InlineData(BrowserFixture.WebKit)]
    public async Task Hold_PastTheDelayWhileTheMainThreadWasBusyAtTheTouch_RaisesLongPressOnRelease(string browserName)
    {
        // Arrange
        await using var page = await LongPressPage.Open(browserFixture.GetBrowser(browserName));
        await page.MoveToButton();
        await page.BlockMainThread(_busyAtTouchDuration);
        await Task.Delay(_busyLoopStartDelay);

        // Act
        await page.Press();
        await page.WaitUntilHeldFor(_holdPastTheDelayWithALateTimer);
        var longPressCountBeforeTheRelease = await page.GetLongPressCount();
        await page.Release();

        // Assert
        longPressCountBeforeTheRelease.ShouldBe(0, RaisedBeforeTheReleaseMessage);
        var longPressCount = await page.GetLongPressCount();
        longPressCount.ShouldBe(1);
    }

    /// <summary>A longpress raised on release still swallows the click after it when its listener prevents the default.</summary>
    [Theory]
    [InlineData(BrowserFixture.Chromium)]
    [InlineData(BrowserFixture.WebKit)]
    public async Task Hold_PreventedLongPressRaisedOnRelease_SwallowsTheClick(string browserName)
    {
        // Arrange
        await using var page = await LongPressPage.Open(browserFixture.GetBrowser(browserName), shouldPreventDefault: true);
        await page.MoveToButton();
        await page.BlockMainThread(_busyAtTouchDuration);
        await Task.Delay(_busyLoopStartDelay);

        // Act
        await page.Press();
        await page.WaitUntilHeldFor(_holdPastTheDelayWithALateTimer);
        var longPressCountBeforeTheRelease = await page.GetLongPressCount();
        await page.Release();
        await Task.Delay(_settleDuration);

        // Assert
        longPressCountBeforeTheRelease.ShouldBe(0, RaisedBeforeTheReleaseMessage);
        var longPressCount = await page.GetLongPressCount();
        longPressCount.ShouldBe(1);
        var clickCount = await page.GetClickCount();
        clickCount.ShouldBe(0);
    }

    /// <summary>
    /// Released past the delay while the main thread is busy, the pointerup and the timer's next frame both wait behind
    /// it. Whichever runs first, the press raises exactly one longpress.
    /// </summary>
    [Theory]
    [InlineData(BrowserFixture.Chromium)]
    [InlineData(BrowserFixture.WebKit)]
    public async Task Hold_ReleasedPastTheDelayWhileTheMainThreadIsBusy_RaisesOneLongPress(string browserName)
    {
        // Arrange
        await using var page = await LongPressPage.Open(browserFixture.GetBrowser(browserName));
        await page.MoveToButton();
        await page.Press();

        // Act
        await page.BlockMainThread(_busyAcrossTheReleaseDuration);
        await Task.Delay(_busyLoopStartDelay);
        await page.ReleaseAfter(_holdReleasedWhileBusy);
        await Task.Delay(_settleDuration);

        // Assert
        var longPressCount = await page.GetLongPressCount();
        longPressCount.ShouldBe(1);
    }
}
