﻿const keyDownHandler = function(event) {
    var serializeEvent = function (event) {
        if (!event) {
            return null;
        }

        var o = {
            altKey: event.altKey,
            bubbles: event.bubbles,
            cancelBubble: event.cancelBubble,
            cancelable: event.cancelable,
            charCode: event.charCode,
            code: event.code,
            composed: event.composed,
            ctrlKey: event.ctrlKey,
            currentTarget: event.currentTarget,
            defaultPrevented: event.defaultPrevented,
            detail: event.detail,
            eventPhase: event.eventPhase,
            isComposing: event.isComposing,
            isTrusted: event.isTrusted,
            key: event.key,
            keyCode: event.keyCode,
            location: event.location,
            metaKey: event.metaKey,
            path: event.path,
            repeat: event.repeat,
            returnValue: event.returnValue,
            shiftKey: event.shiftKey,
            target: event.target,
            timeStamp: event.timeStamp,
            type: event.type,
            which: event.which,
            x: event.x,
            y: event.y
        };

        return o;
    }
    DotNet.invokeMethodAsync('BrigittaBlazor', 'OnKeyDown', serializeEvent(event));
    console.log(event);

    if ((event.ctrlKey && !event.key === 'a') || event.altKey || event.metaKey) {
        event.preventDefault();
    }
}

document.addEventListener('keydown', function (e) { keyDownHandler(e) });