document.addEventListener('input', function (event) {
    if (!event.target.classList.contains('dsm-search-input')) return;
    filterList(event.target.closest('[data-dsm-list]'));
});

document.addEventListener('click', function (event) {
    const clearButton = event.target.closest('[data-dsm-clear]');
    if (clearButton) {
        const panel = clearButton.closest('[data-dsm-list]');
        const input = panel?.querySelector('.dsm-search-input');
        if (input) input.value = '';
        filterList(panel);
        return;
    }

    const dismissToast = event.target.closest('[data-toast-dismiss]');
    if (dismissToast) {
        dismissToast.closest('[data-dsm-toast]')?.remove();
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const toast = document.querySelector('[data-dsm-toast]');
    if (toast) {
        setTimeout(function () {
            toast.remove();
        }, 4500);
    }
});

function filterList(panel) {
    if (!panel) return;
    const search = (panel.querySelector('.dsm-search-input')?.value || '').toLowerCase().trim();
    panel.querySelectorAll('.dsm-search-row').forEach(function (row) {
        row.style.display = row.innerText.toLowerCase().includes(search) ? '' : 'none';
    });
}
