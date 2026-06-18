// Detects when a newer build has been deployed to the server and reloads automatically so the
// client always runs the latest version. Blazor Server keeps an open SignalR circuit + cached
// static assets, so without this an open tab would keep showing the old version until a manual
// reload. We poll a tiny /app-version endpoint and compare it to the version the page loaded with.
//
// NOTE: this auto-reloads with no prompt. A reload drops any in-memory (server-side) cart/sale.
// That is acceptable for the demo; if a sale-in-progress must be protected later, gate the reload
// on cart state (or switch back to a Refresh banner like PlatinumAuth uses).
(function () {
    var meta = document.querySelector('meta[name="app-version"]');
    if (!meta) return;

    var loadedVersion = (meta.getAttribute('content') || '').trim();
    if (!loadedVersion) return;

    var reloading = false;

    function check() {
        if (reloading) return;
        fetch('app-version', { cache: 'no-store' })
            .then(function (r) { return r.ok ? r.text() : null; })
            .then(function (v) {
                if (v && v.trim() && v.trim() !== loadedVersion) {
                    reloading = true;          // guard against double-reload
                    location.reload();         // new page loads with the new version meta -> no loop
                }
            })
            .catch(function () { /* server mid-deploy or offline — try again next tick */ });
    }

    // Poll periodically, and also whenever the tab regains focus / becomes visible.
    setInterval(check, 30 * 1000);
    window.addEventListener('focus', check);
    document.addEventListener('visibilitychange', function () { if (!document.hidden) check(); });
})();
