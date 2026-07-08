// ── Auto-dismiss Bootstrap toasts ────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.toast').forEach(function (el) {
        var toast = new bootstrap.Toast(el, { delay: 4000 });
        toast.show();
    });
});

// ── Highlight active sidebar link based on current URL ────────────
document.addEventListener('DOMContentLoaded', function () {
    const path  = window.location.pathname.toLowerCase();
    const links = document.querySelectorAll('.sidebar-nav .nav-link');

    links.forEach(function (link) {
        const href = link.getAttribute('href')?.toLowerCase();
        if (href && path.startsWith(href) && href !== '/') {
            link.classList.add('active');
        }
    });
});

// ── Prevent double-form-submit on all forms ───────────────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            const btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Please wait...';
            }
        });
    });
});
