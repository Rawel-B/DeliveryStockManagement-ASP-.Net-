document.addEventListener('input', function (event) {
    if (!event.target.classList.contains('dsm-search-input')) return;
    const panel = event.target.closest('[data-dsm-list]');
    filterList(panel);
    updateClearButton(panel);
});

document.addEventListener('change', function (event) {
    if (!event.target.closest('.table-filters')) return;
    const panel = event.target.closest('[data-dsm-list]');
    updateClearButton(panel);
});

document.addEventListener('click', function (event) {
    const accountToggle = event.target.closest('[data-account-toggle]');
    if (accountToggle) {
        const menu = accountToggle.closest('[data-account-menu]');
        const isOpen = menu?.classList.toggle('is-open') ?? false;
        accountToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        return;
    }

    if (!event.target.closest('[data-account-menu]')) {
        closeAccountMenus();
    }

    const iconButton = event.target.closest('[data-profile-icon-option]');
    if (iconButton) {
        const icon = iconButton.getAttribute('data-profile-icon-option') || 'pi-user';
        localStorage.setItem('dsm.profileIcon', icon);
        setProfileIcon(icon);
        document.querySelectorAll('[data-profile-icon-option]').forEach(function (button) {
            button.classList.toggle('active-icon', button === iconButton);
        });
        const input = document.querySelector('[data-profile-icon-input]');
        if (input) input.value = icon;
        return;
    }


    const expandableRow = event.target.closest('.dsm-list-panel .table-row');
    if (expandableRow && !event.target.closest('a, button, input, select, textarea, label')) {
        expandableRow.closest('.row-shell')?.classList.toggle('is-expanded');
        return;
    }

    const clearButton = event.target.closest('[data-dsm-clear]');
    if (clearButton) {
        const panel = clearButton.closest('[data-dsm-list]');
        const input = panel?.querySelector('.dsm-search-input');
        if (input) input.value = '';
        panel?.querySelectorAll('.table-filters select').forEach(function (select) {
            if (select.name && !select.hasAttribute('data-keep-on-clear')) select.value = '';
        });
        filterList(panel);
        updateClearButton(panel);
        return;
    }

    const dismissToast = event.target.closest('[data-toast-dismiss]');
    if (dismissToast) {
        hideToast(dismissToast.closest('[data-dsm-toast]'));
    }
});

document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape') {
        closeAccountMenus();
        document.querySelectorAll('[data-dsm-toast]').forEach(hideToast);
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const savedIcon = localStorage.getItem('dsm.profileIcon') || document.querySelector('[data-profile-icon-input]')?.value || 'pi-user';
    setProfileIcon(savedIcon);

    document.querySelectorAll('[data-profile-icon-option]').forEach(function (button) {
        button.classList.toggle('active-icon', button.getAttribute('data-profile-icon-option') === savedIcon);
    });

    const input = document.querySelector('[data-profile-icon-input]');
    if (input) input.value = savedIcon;

    document.querySelectorAll('[data-dsm-list]').forEach(function (panel) {
        updateClearButton(panel);
        filterList(panel);
    });

    document.querySelectorAll('[data-dsm-toast]').forEach(function (toast) {
        setTimeout(function () {
            hideToast(toast);
        }, 3000);
    });
});

function closeAccountMenus() {
    document.querySelectorAll('[data-account-menu].is-open').forEach(function (menu) {
        menu.classList.remove('is-open');
        menu.querySelector('[data-account-toggle]')?.setAttribute('aria-expanded', 'false');
    });
}

function setProfileIcon(icon) {
    const safeIcon = icon || 'pi-user';
    document.querySelectorAll('[data-profile-icon-current]').forEach(function (element) {
        element.className = 'pi ' + safeIcon;
    });
}

function filterList(panel) {
    if (!panel) return;
    const search = (panel.querySelector('.dsm-search-input')?.value || '').toLowerCase().trim();
    panel.querySelectorAll('.dsm-search-row').forEach(function (row) {
        row.style.display = row.innerText.toLowerCase().includes(search) ? '' : 'none';
    });
}

function updateClearButton(panel) {
    if (!panel) return;
    const searchValue = (panel.querySelector('.dsm-search-input')?.value || '').trim();
    const selectHasValue = Array.from(panel.querySelectorAll('.table-filters select')).some(function (select) {
        return (select.value || '').trim().length > 0;
    });
    const shouldShow = searchValue.length > 0 || selectHasValue;
    panel.querySelectorAll('[data-dsm-clear]').forEach(function (button) {
        button.classList.toggle('is-hidden', !shouldShow);
        button.setAttribute('aria-hidden', shouldShow ? 'false' : 'true');
        button.tabIndex = shouldShow ? 0 : -1;
    });
}

function hideToast(toast) {
    if (!toast) return;
    toast.classList.add('is-hiding');
    setTimeout(function () {
        toast.remove();
    }, 180);
}
