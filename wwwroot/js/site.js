// SchoolApp – site.js

// Auto-dismiss alerts after 4 seconds
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.alert-dismissible').forEach(el => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert?.close();
        }, 4000);
    });
});

// ── Notification Panel ─────────────────────────────────────────────────────
(function () {
    const btn   = document.getElementById('notifBtn');
    const panel = document.getElementById('notifPanel');
    const badge = document.getElementById('notifCount');
    const markAll = document.getElementById('markAllRead');

    if (!btn || !panel) return;

    btn.addEventListener('click', e => {
        e.stopPropagation();
        panel.classList.toggle('show');
    });

    document.addEventListener('click', e => {
        if (!panel.contains(e.target) && e.target !== btn)
            panel.classList.remove('show');
    });

    if (markAll) {
        markAll.addEventListener('click', () => {
            document.querySelectorAll('.notif-item.unread').forEach(x => x.classList.remove('unread'));
            if (badge) { badge.style.display = 'none'; }
        });
    }
})();

// ── Auto-dismiss alerts ────────────────────────────────────────────────────
document.querySelectorAll('.alert-dismissible').forEach(alert => {
    setTimeout(() => {
        const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
        if (bsAlert) bsAlert.close();
    }, 5000);
});
