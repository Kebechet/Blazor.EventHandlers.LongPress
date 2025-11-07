using Microsoft.AspNetCore.Components.Web;

namespace Blazor.EventHandlers.LongPress.Extensions;

public static class LongPressEventArgsExtensions
{
    public static MouseEventArgs ToMouseEventArgs(this LongPressEventArgs longPressEventArgs)
    {
        return new MouseEventArgs
        {
            ScreenX = longPressEventArgs.ScreenX,
            ScreenY = longPressEventArgs.ScreenY,
            ClientX = longPressEventArgs.ClientX,
            ClientY = longPressEventArgs.ClientY,
            OffsetX = longPressEventArgs.OffsetX,
            OffsetY = longPressEventArgs.OffsetY,
            PageX = longPressEventArgs.PageX,
            PageY = longPressEventArgs.PageY,
            Type = longPressEventArgs.Type ?? string.Empty,
        };
    }
}
