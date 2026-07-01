/**
 * show msg where toast id exists
 */
document.addEventListener("DOMContentLoaded", function () {
    showToastIfExist('successToast');
    showToastIfExist('errorToast');
});

/**
 * showToastIfExist 
 * @param {string} elementId 
 */
function showToastIfExist(elementId) {
    const toastEl = document.getElementById(elementId);
    if (toastEl) {
        const toast = new bootstrap.Toast(toastEl);
        toast.show();
    }
}

/**
 * Automatically opens a popup model if error toasts are found on the page
 */
document.addEventListener("DOMContentLoaded", function () {
    const hasError = document.querySelector('.alert-danger') || document.getElementById('errorToast');
    if (hasError) {
        const checkbox = document.getElementById('popupToggle');
        if (checkbox) checkbox.checked = true;
    }
});