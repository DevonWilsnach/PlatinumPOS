// Prints arbitrary HTML (e.g. a Z-Report or receipt) without disturbing the Blazor app DOM.
// We render the markup into a hidden, isolated iframe and call print() on it. An iframe is used
// instead of window.open() because pop-up blockers routinely block the latter (and the Blazor
// Server round-trip means the call no longer counts as a direct user gesture), which looked to
// users like the print button "hanging" with nothing happening.
window.platinumPrint = {
    printHtml: function (html) {
        // Remove any leftover frame from a previous print that may not have torn down yet.
        var existing = document.getElementById('platinum-print-frame');
        if (existing && existing.parentNode) existing.parentNode.removeChild(existing);

        var frame = document.createElement('iframe');
        frame.id = 'platinum-print-frame';
        frame.setAttribute('aria-hidden', 'true');
        // Off-screen rather than 0x0: some browsers won't lay out / render a zero-size iframe,
        // which leaves Chrome's print preview stuck on "Loading preview...".
        frame.style.position = 'fixed';
        frame.style.left = '-10000px';
        frame.style.top = '0';
        frame.style.width = '380px';
        frame.style.height = '600px';
        frame.style.border = '0';
        document.body.appendChild(frame);

        var win = frame.contentWindow;
        var printed = false;

        var cleanup = function () {
            var f = document.getElementById('platinum-print-frame');
            if (f && f.parentNode) f.parentNode.removeChild(f);
        };

        var fire = function () {
            if (printed) return;
            printed = true;
            // Tear the frame down only AFTER the print dialog closes. Removing it on a fixed
            // timer (the old behaviour) could yank the document out from under the preview and
            // leave it loading forever.
            win.onafterprint = function () { setTimeout(cleanup, 200); };
            try {
                win.focus();
                win.print();
            } catch (e) {
                cleanup();
                throw e;
            }
            // Safety net: if onafterprint never fires (some browsers), reclaim the frame later.
            setTimeout(cleanup, 60000);
        };

        var doc = win.document;
        doc.open();
        doc.write(html);
        doc.close();

        // A document written via document.write is usually 'complete' immediately, but wait a
        // tick (and also hook onload) so fonts/layout settle before the dialog opens.
        frame.onload = function () { setTimeout(fire, 200); };
        setTimeout(fire, 400); // fallback in case onload doesn't fire for the written doc
    }
};
