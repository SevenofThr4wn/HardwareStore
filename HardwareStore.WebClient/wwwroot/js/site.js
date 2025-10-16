window.addEventListener('load', () => {

    applyTheme();

    document.getElementById('btnTheme').addEventListener('click', () => switchTheme());
});

function applyTheme() {
    let savedTheme = localStorage.getItem("Theme");

    if (!savedTheme) {

        let hour = new Date().GetHours();

        if (hour >= 6 && hour < 18) {
            savedTheme = "Primary";

        } else {
            savedTheme = "Secondary";
        }

        localStorage.setItem("Theme", savedTheme);
    }

    setTheme(savedTheme);
}

function switchTheme() {
    let currentTheme = localStorage.getItem("Theme");
    let newTheme = currentTheme === "Secondary" ? "Primary" : "Secondary";
    localStorage.setItem("Theme", newTheme);
    setTheme(newTheme);
}

function setTheme(theme) {
    const themeLink = document.getElementById('themeStyle');
    if (!themeLink) return;

    if (theme === "Secondary") {
        themeLink.setAttribute("href", "/css/secondary-theme.css");
    } else {
        themeLink.setAttribute("href", "/css/primary-theme.css");
    }
}