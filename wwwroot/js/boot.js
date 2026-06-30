// App boot helpers, kept in an EXTERNAL file (not inline) so the Content-Security-Policy
// can stay strict — script-src 'self' allows this file.
// Handles: dismissing the splash screen after Blazor loads.

// Dismiss the animated splash after 3 s: fade out, then stop the canvas animation loop and
// remove the element so it doesn't consume memory/GPU once the app is running.
window.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var splash = document.getElementById('splash-screen');
        if (!splash) return;
        splash.classList.add('splash-fade');
        setTimeout(function () {
            splash.style.display = 'none';
            if (typeof window._splashStop === 'function') window._splashStop();
        }, 600);
    }, 3000);
});
