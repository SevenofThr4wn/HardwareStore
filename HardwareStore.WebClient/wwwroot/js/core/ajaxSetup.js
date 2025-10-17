// ajax-setup.js
$(function () {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajaxSetup({
        headers: {
            'RequestVerificationToken': token
        },
        error: function (xhr, status, error) {
            console.error(`AJAX Error [${status}]: ${error}`);
        }
    });
});