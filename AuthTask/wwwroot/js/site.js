document.getElementById('changeMode').addEventListener('change', (ev) => {
    if (!ev.target.checked) {
        ev.target.checked = false;
        document.documentElement.setAttribute('data-bs-theme', 'light');
        localStorage.setItem('page-theme', 'light');
    } else {
        ev.target.checked = true;
        document.documentElement.setAttribute('data-bs-theme', 'dark');
        localStorage.setItem('page-theme', 'dark');
    }
});


document.addEventListener('DOMContentLoaded', function () {
    let themeMode = 'dark';

    if (document.documentElement) {
        if (document.documentElement.hasAttribute('data-bs-theme')) {
            if (localStorage.getItem('page-theme') !== null) {
                themeMode = localStorage.getItem('page-theme');
            }
        }
    }

    document.getElementById('changeMode').checked = themeMode === 'dark' ? true : false;
    document.documentElement.setAttribute('data-bs-theme', themeMode);
});