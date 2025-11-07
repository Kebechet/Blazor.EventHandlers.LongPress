using Microsoft.AspNetCore.Components.Web;

namespace Blazor.EventHandlers.LongPress.Extensions;

public static class MouseEventArgsExtensions
{
    public static LongPressEventArgs ToLongPressEventArgs(this MouseEventArgs mouseEventArgs)
    {
        return new LongPressEventArgs
        {
            ScreenX = mouseEventArgs.ScreenX,
            ScreenY = mouseEventArgs.ScreenY,
            ClientX = mouseEventArgs.ClientX,
            ClientY = mouseEventArgs.ClientY,
            OffsetX = mouseEventArgs.OffsetX,
            OffsetY = mouseEventArgs.OffsetY,
            PageX = mouseEventArgs.PageX,
            PageY = mouseEventArgs.PageY,
            Type = mouseEventArgs.Type
        };
    }
}
