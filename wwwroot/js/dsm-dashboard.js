document.addEventListener('input', function (event) {
    if (!event.target.classList.contains('dsm-search-input')) return;
    filterList(event.target.closest('[data-dsm-list]'));
});

document.addEventListener('click', function (event) {
    const clearButton = event.target.closest('[data-dsm-clear]');
    if (!clearButton) return;
    const panel = clearButton.closest('[data-dsm-list]');
    const input = panel?.querySelector('.dsm-search-input');
    if (input) input.value = '';
    filterList(panel);
});

function filterList(panel) {
    if (!panel) return;
    const search = (panel.querySelector('.dsm-search-input')?.value || '').toLowerCase().trim();
    panel.querySelectorAll('.dsm-search-row').forEach(function (row) {
        row.style.display = row.innerText.toLowerCase().includes(search) ? '' : 'none';
    });
}
