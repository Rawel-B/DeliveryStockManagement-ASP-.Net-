document.addEventListener('click', function (event) {
    const toggle = event.target.closest('[data-auth-ticket-toggle]');
    if (!toggle) return;
    const card = toggle.closest('.auth-card');
    const form = card?.querySelector('[data-auth-ticket-form]');
    if (form) form.classList.toggle('hidden');
});

document.addEventListener('DOMContentLoaded', function () {
    const toast = document.querySelector('[data-dsm-toast]');
    if (toast) {
        setTimeout(function () {
            toast.remove();
        }, 4500);
    }
});
