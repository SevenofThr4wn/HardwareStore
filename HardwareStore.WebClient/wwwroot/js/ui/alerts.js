// ===============================
// alerts.js
// Global alert and confirmation system using SweetAlert2
// ===============================

/**
 * Base configuration for all alerts.
 * You can customize colors, icons, and button styles here.
 */
const AlertConfig = {
    confirmButtonColor: '#3085d6',
    cancelButtonColor: '#d33',
    allowOutsideClick: false,
    allowEscapeKey: true,
    customClass: {
        popup: 'rounded-4 shadow-lg',
        confirmButton: 'btn btn-primary px-4',
        cancelButton: 'btn btn-danger px-4'
    }
};

/**
 * Shows a basic message alert.
 * @param {string} title - The main heading.
 * @param {string} message - The descriptive text.
 * @param {string} type - "success" | "error" | "warning" | "info" | "question"
 */
function showAlert(title, message = '', type = 'info') {
    return Swal.fire({
        ...AlertConfig,
        icon: type,
        title: title,
        text: message,
        confirmButtonText: 'OK'
    });
}

/**
 * Shows a confirmation dialog with Yes/No options.
 * @param {string} message - The question to ask the user.
 * @param {function} onConfirm - The callback to execute if confirmed.
 * @param {string} title - Optional title.
 */
function showConfirm(message, onConfirm, title = 'Are you sure?') {
    Swal.fire({
        ...AlertConfig,
        title: title,
        text: message,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes',
        cancelButtonText: 'No'
    }).then(result => {
        if (result.isConfirmed && typeof onConfirm === 'function') {
            onConfirm();
        }
    });
}

/**
 * Shows a non-blocking toast message in the corner.
 * @param {string} message - The content to show.
 * @param {string} type - "success" | "error" | "info" | "warning"
 * @param {number} duration - How long the toast stays visible.
 */
function showToast(message, type = 'info', duration = 3000) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: duration,
        timerProgressBar: true,
        customClass: { popup: 'colored-toast' },
        didOpen: toast => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    Toast.fire({
        icon: type,
        title: message
    });
}

/**
 * Shows a loading indicator with optional text.
 * @param {string} message - Loading message.
 */
function showLoading(message = 'Processing...') {
    Swal.fire({
        ...AlertConfig,
        title: message,
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

/**
 * Closes any currently active alert (e.g. after AJAX success).
 */
function closeAlert() {
    Swal.close();
}
