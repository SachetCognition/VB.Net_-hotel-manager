window.hotelTheme = {
    get: function () {
        try {
            return localStorage.getItem('hm-dark-mode');
        } catch {
            return null;
        }
    },
    set: function (isDark) {
        try {
            localStorage.setItem('hm-dark-mode', isDark ? 'true' : 'false');
        } catch {
            /* ignore storage errors (private mode, etc.) */
        }
    }
};
