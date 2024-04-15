# Blazor.EventHandlers.LongPress

This package contains `@onlongpress` event handler built from [John Doherty's](https://github.com/john-doherty) JS [long-press](https://github.com/john-doherty/long-press-event) 

**Motivation:** For similar functionality you can also use `oncontextmenu` what is built-in event. But it doesnt work on iOS devices. [Source](https://caniuse.com/mdn-api_element_contextmenu_event)

- Before using the JS, I renamed triggered event name from `long-press` to `longpress` to be able to use it in Blazor.
- I have also changed default show time from `1500ms` to `400ms` Based on [android longpress implementation](https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#:~:text=public%20static%20final%20int%20DEFAULT_LONG_PRESS_TIMEOUT).
  - iOS has [500ms](https://developer.apple.com/documentation/uikit/touches_presses_and_gestures/handling_uikit_gestures/handling_long-press_gestures) 
- In case you would like to specify your custom time, you can do so by adding tag attribute with your custom time like: 
``` 
<div @onlongpress="LongPressHandler" data-long-press-delay="100"> 
```

**TIP:** For better usage experience put: `@using Blazor.EventHandlers.LongPress` into `_Imports.razor` file.

**TIP2** For combination with `@onlongpress` consider also use of [@onshortclick](https://github.com/Kebechet/Blazor.EventHandlers.ShortClick)

## ⚠️ Auto-initializers
- In previous MAUI versions [JS initializers were not working](https://github.com/dotnet/maui/issues/15201). So if you dont see text "Registering LongPress.js" in your JS console then you have to register this event handler manually in your `index.html`. At the end after blazor script put this:
```
<script defer src="_content/Kebechet.Blazor.EventHandlers.LongPress/LongPress.js"></script>
<script async>
	window.onload = function () {
		console.log("Registering LongPress.js");

		Blazor.registerCustomEventType('longpress', {
			createEventArgs: event => {
				return {
					bubbles: event.bubbles,
					cancelable: event.cancelable,
					screenX: event.detail.screenX,
					screenY: event.detail.screenY,
					clientX: event.detail.clientX,
					clientY: event.detail.clientY,
					offsetX: event.detail.offsetX,
					offsetY: event.detail.offsetY,
					pageX: event.detail.pageX,
					pageY: event.detail.pageY,
					sourceElement: event.srcElement.localName,
					targetElement: event.target.localName,
					timeStamp: event.timeStamp,
					type: event.type,
				};
			}
		});

		console.log("Registering ShortClick.js");
		Blazor.registerCustomEventType('shortclick', {
			createEventArgs: event => {
				return {
					screenX: event.detail.screenX,
					screenY: event.detail.screenY,
					clientX: event.detail.clientX,
					clientY: event.detail.clientY,
					offsetX: event.detail.offsetX,
					offsetY: event.detail.offsetY,
					pageX: event.detail.pageX,
					pageY: event.detail.pageY,
					movementX: event.detail.movementX,
					movementY: event.detail.movementY,
					button: event.detail.button,
					buttons: event.detail.buttons,
					ctrlKey: event.detail.ctrlKey,
					shiftKey: event.detail.shiftKey,
					altKey: event.detail.altKey,
					metaKey: event.detail.metaKey,
					type: event.detail.type,
				};
			}
		});
	};
</script>
```
- and then it should work as expected

## Used resources
- MSDN 
  - [docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-7.0)
  - [docs](https://learn.microsoft.com/en-us/mobile-blazor-bindings/walkthroughs/hybrid-hello-world#wwwroot-static-web-assets-folder) 
- ASP.NET Core [repo](https://github.com/dotnet/aspnetcore/tree/main/src/Components/Web/src/Web)
- Fluent UI Blazor [repo](https://github.com/microsoft/fluentui-blazor/blob/1aa0e2fdd144c3b243b387d90d5a6499995436df/src/Microsoft.Fast.Components.FluentUI/wwwroot/Microsoft.Fast.Components.FluentUI.lib.module.js)

# License
This repository is licensed with the [MIT](LICENSE.txt) license.
