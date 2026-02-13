// Document Ready
document.addEventListener('DOMContentLoaded', function () {
    initializeValidation();
    initializeToasts();
});

// Initialize form validation
function initializeValidation() {
    const forms = document.querySelectorAll('form[novalidate]');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
}

// Initialize toast notifications
function initializeToasts() {
    const toastElements = document.querySelectorAll('.toast');
    toastElements.forEach(toastElement => {
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    });
}

// Format time input
function formatTime(timeString) {
    if (!timeString) return '';
    const [hours, minutes] = timeString.split(':');
    return `${hours}:${minutes}`;
}

// Calculate hours between two times
function calculateHours(timeIn, timeOut) {
    if (!timeIn || !timeOut) return 0;

    const [inHours, inMinutes] = timeIn.split(':').map(Number);
    const [outHours, outMinutes] = timeOut.split(':').map(Number);

    const inTotalMinutes = inHours * 60 + inMinutes;
    const outTotalMinutes = outHours * 60 + outMinutes;

    let diffMinutes = outTotalMinutes - inTotalMinutes;
    if (diffMinutes < 0) {
        diffMinutes += 24 * 60;
    }

    return (diffMinutes / 60).toFixed(2);
}

// Confirm action
function confirmAction(message) {
    return confirm(message);
}

// Show error message
function showError(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    const container = document.querySelector('main');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
    }
}

// Show success message
function showSuccess(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-success alert-dismissible fade show';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    const container = document.querySelector('main');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
    }
}

// Format date
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return date.toLocaleDateString('en-US', options);
}

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Get query parameter
function getQueryParameter(name) {
    const params = new URLSearchParams(window.location.search);
    return params.get(name);
}

// Dynamically update total hours based on time in/out
document.addEventListener('change', function (e) {
    if (e.target.name === 'TimeIn' || e.target.name === 'TimeOut') {
        const timeInInput = document.querySelector('input[name="TimeIn"]');
        const timeOutInput = document.querySelector('input[name="TimeOut"]');
        const hoursDisplay = document.querySelector('.total-hours');

        if (timeInInput && timeOutInput && hoursDisplay) {
            const hours = calculateHours(timeInInput.value, timeOutInput.value);
            hoursDisplay.textContent = `Total Hours: ${hours}`;
        }
    }
});

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});