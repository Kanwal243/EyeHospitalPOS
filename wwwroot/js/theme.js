function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
}

function getTheme() {
    return localStorage.getItem('theme') || 'light';
}

// Initialize theme on load
(function() {
    const theme = getTheme();
    document.documentElement.setAttribute('data-theme', theme);
})();
