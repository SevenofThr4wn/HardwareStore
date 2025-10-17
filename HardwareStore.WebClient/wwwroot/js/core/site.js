
let theme = localStorage.getItem("Theme");
if (theme === "Secondary") {
    document.getElementById('themeStyle').setAttribute("href", "/css/secondary-theme.css")
}
window.addEventListener('load', () => {
    document.getElementById('btnTheme').addEventListener('click', () => switchTheme())
})

function switchTheme() {
    let currentTheme = localStorage.getItem("Theme");

    if (currentTheme === "Secondary") {
        localStorage.setItem("Theme", "Primary")
        document.getElementById('themeStyle').setAttribute("href", "/css/primary-theme.css")
    }
    else {
        localStorage.setItem("Theme", "Secondary")
        document.getElementById('themeStyle').setAttribute("href", "/css/secondary-theme.css")
    }
}
