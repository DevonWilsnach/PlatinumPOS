// Prints arbitrary HTML (e.g. a Z-Report or receipt) without disturbing the Blazor app DOM.
// We render the markup into a hidden, isolated iframe and call print() on it. An iframe is used
// instead of window.open() because pop-up blockers routinely block the latter, which looked to
// users like the print button "hanging" with nothing happening.
window.platinumPrint = {
    printHtml: function (html) {
        // Remove any leftover frame from a previous print that may not have torn down yet.
        var existing = document.getElementById('platinum-print-frame');
        if (existing) existing.parentNode.removeChild(existing);

        var frame = document.createElement('iframe');
        frame.id = 'platinum-print-frame';
        frame.setAttribute('aria-hidden', 'true');
        frame.style.position = 'fixed';
        frame.style.right = '0';
        frame.style.bottom = '0';
        frame.style.width = '0';
        frame.style.height = '0';
        frame.style.border = '0';
        document.body.appendChild(frame);

        var doc = frame.contentWindow.document;
        doc.open();
        doc.write(html);
        doc.close();

        var cleanup = function () {
            if (frame && frame.parentNode) frame.parentNode.removeChild(frame);
        };

        var doPrint = function () {
            // Give the browser a tick to lay out fonts/content before invoking the dialog.
            setTimeout(function () {
                try {
                    frame.contentWindow.focus();
                    frame.contentWindow.print();
                } finally {
                    // Tear the frame down after the (modal) print dialog returns.
                    setTimeout(cleanup, 1000);
                }
            }, 150);
        };

        if (doc.readyState === 'complete') {
            doPrint();
        } else {
            frame.onload = doPrint;
        }
    }
};
