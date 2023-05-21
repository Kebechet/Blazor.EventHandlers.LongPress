# Blazor.EventHandlers.LongPress

This package contains `@onlongpress` event handler built from [John Doherty's](https://github.com/john-doherty) JS [long-press](https://github.com/john-doherty/long-press-event) 

- Before using the JS, I renamed triggered event name from `long-press` to `longpress` to be able to use it in Blazor.
- I have also changed default show time from `1500ms` to `500ms`. In case you would like to specify your custom time, you can do so by adding tag attribute with your custom time like: 
``` 
<div @onlongpress="LongPressHandler" data-long-press-delay="100"> 
```

*TIP:* For better usage experience put: `@using Blazor.EventHandlers.LongPress` into `_Imports.razor` file.

## Used resources
- MSDN [docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-7.0)
- ASP.NET Core [repo](https://github.com/dotnet/aspnetcore/tree/main/src/Components/Web/src/Web)
- Fluent UI Blazor [repo](https://github.com/microsoft/fluentui-blazor/blob/1aa0e2fdd144c3b243b387d90d5a6499995436df/src/Microsoft.Fast.Components.FluentUI/wwwroot/Microsoft.Fast.Components.FluentUI.lib.module.js)

# License
This repository is licensed with the [MIT](LICENSE.txt) license.
