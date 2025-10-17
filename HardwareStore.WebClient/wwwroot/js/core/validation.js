$(function () {
    if ($.validator) {
        console.log("Validation initialized.");
        $.validator.unobtrusive.parse(document);
    }
});