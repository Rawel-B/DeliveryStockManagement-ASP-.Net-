document.addEventListener('input', function (event) {
    if (!event.target.classList.contains('dsm-search-input')) return;
    filterList(event.target.closest('[data-dsm-list]'));
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
        document.querySelectorAll('[data-account-menu].is-open').forEach(function (menu) {
            menu.classList.remove('is-open');
            menu.querySelector('[data-account-toggle]')?.setAttribute('aria-expanded', 'false');
        });
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
    const savedIcon = localStorage.getItem('dsm.profileIcon') || document.querySelector('[data-profile-icon-input]')?.value || 'pi-user';
    setProfileIcon(savedIcon);

    document.querySelectorAll('[data-profile-icon-option]').forEach(function (button) {
        button.classList.toggle('active-icon', button.getAttribute('data-profile-icon-option') === savedIcon);
    });

    const input = document.querySelector('[data-profile-icon-input]');
    if (input) input.value = savedIcon;

    const toast = document.querySelector('[data-dsm-toast]');
    if (toast) {
        setTimeout(function () {
            toast.remove();
        }, 4500);
    }
});

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
