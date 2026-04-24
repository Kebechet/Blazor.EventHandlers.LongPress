export function beforeWebStart(options, extensions) {
    beforeStart(options, extensions);
}

export function afterWebStarted(blazor) {
    afterStarted(blazor)
}

export function beforeStart(options, extensions) {
    console.log("Injecting long-press-event.min.js");

	var element = document.createElement('script');
    element.src = "_content/Kebechet.Blazor.EventHandlers.LongPress/long-press-event.min.js";
	element.async = true;
	document.body.appendChild(element);
}

export function afterStarted(blazor) {
    console.log("Registering long-press-event.min.js");

	blazor.registerCustomEventType('longpress', {
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
}
