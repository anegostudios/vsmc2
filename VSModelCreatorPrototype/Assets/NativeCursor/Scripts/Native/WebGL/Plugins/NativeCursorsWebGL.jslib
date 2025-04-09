mergeInto(LibraryManager.library, {
    SetCursorStyle: function (rawStr) {
        var str = UTF8ToString(rawStr);

        var canvas = document.getElementById('unity-canvas');

        if (!canvas || canvas.nodeName !== 'CANVAS') {
            canvas = document.querySelector('canvas');
        }

        if (canvas) {
            canvas.style.cursor = str;
        }
    },
});