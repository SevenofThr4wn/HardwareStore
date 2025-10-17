function showAlert(title, text, icon = 'info') {
    if (window.Swal) {
        Swal.fire({ title, text, icon });
    } else {
        alert(`${title}\n${text}`);
    }
}

function confirmAction(message, callback) {
    if (window.Swal) {
        Swal.fire({
            title: 'Confirm',
            text: message,
            icon: 'question',
            showCancelButton: true
        }).then(result => {
            if (result.isConfirmed) callback();
        });
    } else if (confirm(message)) {
        callback();
    }
}