// Animated splash screen: canvas particle effect that runs before Blazor loads.
// Reads the user's saved dark-mode preference from localStorage so the correct
// palette is applied immediately — no flash between modes on first paint.
// Must remain a CSP-safe external file (no inline scripts).

(function () {
    var isDark = (function () {
        var stored = localStorage.getItem('rw-dark-mode');
        if (stored !== null) return stored === 'true';
        return true; // dark mode is the default
    })();

    var BG       = isDark ? '#050505'              : '#FFFFFF';
    var TRAIL    = isDark ? 'rgba(5,5,5,0.25)'     : 'rgba(255,255,255,0.28)';
    var PARTICLE = isDark ? '0,51,102'             : '0,51,102';
    var P_ALPHA  = isDark ? 0.9                    : 0.45;

    // Write the theme class onto the splash element so CSS can adapt text colours.
    document.addEventListener('DOMContentLoaded', function () {
        var el = document.getElementById('splash-screen');
        if (el) el.classList.add(isDark ? 'splash-dark' : 'splash-light');
    });

    var canvas = document.getElementById('splashCanvas');
    if (!canvas) return;
    var ctx = canvas.getContext('2d');
    var particles = [];

    function Particle(w, h) {
        this.w = w; this.h = h;
        this.spawn(true);
    }
    Particle.prototype.spawn = function (random) {
        if (random) {
            this.x = Math.random() * this.w;
            this.y = Math.random() * this.h;
        } else {
            if (Math.random() > 0.5) {
                this.x = Math.random() > 0.5 ? -10 : this.w + 10;
                this.y = Math.random() * this.h;
            } else {
                this.x = Math.random() * this.w;
                this.y = Math.random() > 0.5 ? -10 : this.h + 10;
            }
        }
        this.base  = Math.random() * 1.2 + 0.3;
        this.r     = this.base;
        this.speed = Math.random() * 0.5 + 0.1;
        this.alpha = 0;
        this.fadingIn = true;
    };
    Particle.prototype.update = function () {
        var dx = this.w / 2 - this.x, dy = this.h / 2 - this.y;
        var dist = Math.sqrt(dx * dx + dy * dy);
        if (dist > 1) { this.x += (dx / dist) * this.speed; this.y += (dy / dist) * this.speed; }
        if (this.fadingIn) { this.alpha += 0.01; if (this.alpha >= 1) { this.alpha = 1; this.fadingIn = false; } }
        if (dist < 250) {
            var pulse = Math.sin((dist / 250) * Math.PI) * 3.5;
            this.r = Math.max(0.1, this.base + pulse);
        } else { this.r = this.base; }
        if (dist < 30) { this.alpha -= 0.05; if (this.alpha <= 0) this.spawn(false); }
    };
    Particle.prototype.draw = function () {
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.r, 0, Math.PI * 2);
        ctx.fillStyle = 'rgba(' + PARTICLE + ',' + (this.alpha * P_ALPHA) + ')';
        ctx.shadowBlur = this.r * 2;
        ctx.shadowColor = 'rgba(' + PARTICLE + ',' + this.alpha + ')';
        ctx.fill();
        ctx.shadowBlur = 0;
    };

    function resize() {
        canvas.width  = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.background = BG;
        particles = [];
        var n = Math.floor((canvas.width * canvas.height) / 9000);
        for (var i = 0; i < n; i++) particles.push(new Particle(canvas.width, canvas.height));
    }
    window.addEventListener('resize', resize);
    resize();

    var running = true;
    function animate() {
        if (!running) return;
        ctx.fillStyle = TRAIL;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        for (var i = 0; i < particles.length; i++) { particles[i].update(); particles[i].draw(); }
        requestAnimationFrame(animate);
    }
    animate();

    // Expose a stop-handle so boot.js can halt the animation loop after Blazor loads.
    window._splashStop = function () { running = false; };
})();
