// TPL Web Application - Main JavaScript File
'use strict';

// Import dependencies
import 'bootstrap/dist/js/bootstrap.bundle.js';
import 'jquery';

// Global application object
window.TPL = window.TPL || {};

// Application initialization
document.addEventListener('DOMContentLoaded', function() {
    console.log('TPL Web Application initialized');
    
    // Initialize Bootstrap components
    initializeBootstrap();
    
    // Initialize common functionality
    initializeCommon();
});

// Bootstrap initialization
function initializeBootstrap() {
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Initialize modals
    const modalTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="modal"]'));
    modalTriggerList.map(function (modalTriggerEl) {
        return new bootstrap.Modal(modalTriggerEl);
    });
}

// Common functionality initialization
function initializeCommon() {
    // Handle form validation
    handleFormValidation();
    
    // Handle AJAX requests
    handleAjaxRequests();
    
    // Handle notifications
    handleNotifications();
}

// Form validation
function handleFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
}

// AJAX request handling
function handleAjaxRequests() {
    // Global AJAX error handler
    $(document).ajaxError(function(event, xhr, settings, error) {
        console.error('AJAX Error:', error);
        
        if (xhr.status === 401) {
            // Unauthorized - redirect to login
            window.location.href = '/Account/Login';
        } else if (xhr.status === 403) {
            // Forbidden
            showNotification('دسترسی غیرمجاز', 'error');
        } else if (xhr.status >= 500) {
            // Server error
            showNotification('خطای سرور', 'error');
        }
    });
}

// Notification handling
function handleNotifications() {
    // Auto-hide alerts after 5 seconds
    setTimeout(() => {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
}

// Show notification function
function showNotification(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    const container = document.querySelector('.container-xxl') || document.body;
    container.insertBefore(alertDiv, container.firstChild);
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        const bsAlert = new bootstrap.Alert(alertDiv);
        bsAlert.close();
    }, 5000);
}

// Export for use in other modules
export { showNotification, initializeBootstrap, initializeCommon };
