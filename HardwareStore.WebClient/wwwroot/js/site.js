
//Retrieve and store the Theme value from the browser storage.
let theme = localStorage.getItem("Theme");
//If the them is set to secondary chage it. Otherwise it will stay as the primary theme.
if (theme === "Secondary") {
    document.getElementById('themeStyle').setAttribute("href", "/css/secondary-theme.css")
}
//Add a listener to wait for the page to load.
window.addEventListener('load', () => {
    //Find the theme button and add a listener to run the switch them method when clicked.
    document.getElementById('btnTheme').addEventListener('click', () => switchTheme())
})

function switchTheme() {
    //Retrieve and store the Theme value from the browser storage.
    let currentTheme = localStorage.getItem("Theme");

    if (currentTheme === "Secondary") {
        //Set the theme in the storage and the CSS variables to the primary style
        localStorage.setItem("Theme", "Primary")
        document.getElementById('themeStyle').setAttribute("href", "/css/primary-theme.css")
    }
    else {
        ////Set the theme in the storage and the CSS variables to the secondary style
        localStorage.setItem("Theme", "Secondary")
        document.getElementById('themeStyle').setAttribute("href", "/css/secondary-theme.css")
    }
}
