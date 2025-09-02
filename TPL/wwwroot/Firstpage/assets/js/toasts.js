// For more details see: https://getbootstrap.com/docs/5.0/components/toasts/#usage

document.addEventListener('DOMContentLoaded', function() {
    // Initialize toasts functionality
    const toastElements = document.querySelectorAll('.toast');
    toastElements.forEach(function(element) {
        // Remove debug logging
        new bootstrap.Toast(element);
    });
});
