/**
 * Global AJAX and Modal Handler for Pharmacy Project
 * Handles all popup operations, form submissions, and table updates
 */

$(document).ready(function () {
    // Initialize all event handlers
    initializeAjaxHandlers();
    initializeModalHandlers();
    initializeFormHandlers();
});

// Disable the global loader overlay/spinner when true
// Set to true to remove the overlay/spinner shown in the screenshot
window.__disableGlobalLoader = true;

// Global AJAX setup
function initializeAjaxHandlers() {
    // Set up CSRF token for all AJAX requests
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            if (!csrfSafeMethod(settings.type) && !this.crossDomain) {
                xhr.setRequestHeader("RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
            }
        },
        timeout: 30000 // 30s timeout for all AJAX requests to avoid hanging loader
    });

    // Global AJAX error handler
    $(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
        if (jqXHR.status === 403) {
            showMessage('Access denied. Please check your permissions.', 'error');
        } else if (jqXHR.status === 500) {
            showMessage('Server error occurred. Please try again.', 'error');
        }
        // Ensure loader is hidden on any global AJAX error to avoid stuck spinner
        try { hideLoader(); } catch (e) { console.warn('hideLoader failed in ajaxError', e); }
    });

    // Global ajax start/stop to show/hide loader automatically for any AJAX activity
    if (!window.__disableGlobalLoader) {
        $(document).ajaxStart(function () {
            // small delay so quick requests don't flash the loader
            window.__globalLoaderTimer = setTimeout(function () { showLoader(); }, 150);
        });

        $(document).ajaxStop(function () {
            clearTimeout(window.__globalLoaderTimer);
            try { hideLoader(); } catch (e) { console.warn('hideLoader failed in ajaxStop', e); }
        });
    }
}

// CSRF token validation
function csrfSafeMethod(method) {
    return (/^(GET|HEAD|OPTIONS|TRACE)$/.test(method));
}

// Modal handlers
function initializeModalHandlers() {
    // Handle modal cleanup on close
    $('.modal').on('hidden.bs.modal', function () {
        $(this).find('.modal-body').empty();
        $(this).find('.modal-title span').text('');
        $(this).removeData('bs.modal');
    });

    // Handle modal shown event for focus management
    $('.modal').on('shown.bs.modal', function () {
        $(this).find('input:first').focus();
    });
}

// Form handlers
function initializeFormHandlers() {
    // Handle form submission prevention on Enter key (except for textareas)
    $(document).on('keypress', 'input:not(textarea)', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $(this).closest('form').find('button[type="submit"], input[type="submit"]').first().click();
        }
    });
}

// ===================== ITEM STATUS OPERATIONS =====================

function deleteItemStatus(id) {
    if (confirm('Are you sure you want to delete this item status?')) {
        $.ajax({
            url: '/ItemStatuses/Delete/' + id,
            type: 'POST',
            success: function (result) {
                if (result.success) {
                    showMessage('Item status deleted successfully.', 'success');
                    $('#itemStatusTable').DataTable().ajax.reload();
                } else {
                    showMessage(result.message || 'Failed to delete item status.', 'error');
                }
            },
            error: function () {
                showMessage('An error occurred while deleting the item status.', 'error');
            }
        });
    }
}

function submitForm(form) {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            processData: false,
            contentType: false,
            success: function (result) {
                if (result.success) {
                    $('#form-modal').modal('hide');
                    showMessage('Item status saved successfully.', 'success');
                    $('#itemStatusTable').DataTable().ajax.reload();
                } else {
                    showMessage(result.message || 'Failed to save item status.', 'error');
                }
            },
            error: function () {
                showMessage('An error occurred while saving the item status.', 'error');
            }
        });
    } catch (ex) {
        console.error(ex);
    }
    return false;
}

// ===================== COUNTRY OPERATIONS =====================

// Load Countries List
function loadCountriesList() {
    showLoader();
    $.get('/Countries/GetCountries')
        .done(function (data) {
            if (typeof data === 'string' && data.toLowerCase().indexOf('<form') !== -1 && data.toLowerCase().indexOf('login') !== -1) {
                window.location = '/Account/Login';
                return;
            }
            $('#countriesTableContainer').html(data);
        })
        .fail(function () {
            showMessage('Failed to load countries list.', 'error');
        })
        .always(function () {
            hideLoader();
        });
}

// Open Country Modal
function openCountryModal(id = 0) {
    const isEdit = id > 0;
    const modalTitle = isEdit ? 'Edit Country' : 'Add New Country';

    $('#countryModalLabel span').text(modalTitle);
    // Load form content
    $.get('/Countries/AddOrEdit', { id: id })
        .done(function (data) {
            $('#countryModalBody').html(data);
            var modalEl = document.getElementById('countryModal');
            var bs = bootstrap.Modal.getOrCreateInstance(modalEl);
            bs.show();
            setTimeout(function () { initializeFormValidation('#countryForm'); }, 100);
        })
        .fail(function () {
            showMessage('Failed to load country form.', 'error');
        });
}

// Save Country
function saveCountry() {
    const form = $('#countryForm');
    if (!form.length) { showMessage('Form not found', 'error'); return; }
    const formData = form.serialize();

    if (typeof form.valid === 'function' && !form.valid()) {
        showMessage('Please correct the validation errors.', 'error');
        return;
    }

    const $saveBtn = $('#saveCountryBtn');
    const originalSaveHtml = $saveBtn.html();
    $saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Saving...');

    showLoader();

    $.ajax({ url: '/Countries/AddOrEdit', method: 'POST', data: formData })
        .done(function (response) {
            console.log('saveCountry response:', response);
            if (response && typeof response === 'object' && response.isValid) {
                showMessage(response.message || 'Saved successfully', 'success');
                try { bootstrap.Modal.getInstance(document.getElementById('countryModal')).hide(); } catch (e) { $('#countryModal').modal('hide'); }
                if (response.html) $('#countriesTableContainer').html(response.html); else loadCountriesList();
                return;
            }

            if (typeof response === 'string') {
                // HTML fragment with validation messages
                $('#countryModalBody').html(response);
                initializeFormValidation('#countryForm');
                return;
            }

            showMessage('Unexpected response from server.', 'warning');
            loadCountriesList();
        })
        .fail(function (xhr) {
            console.log('saveCountry failed xhr:', xhr);
            const contentType = xhr.getResponseHeader('Content-Type') || '';
            if (contentType.indexOf('text/html') !== -1 && xhr.responseText) {
                $('#countryModalBody').html(xhr.responseText);
                initializeFormValidation('#countryForm');
                return;
            }
            showMessage('An error occurred while saving the country.', 'error');
        })
        .always(function () {
            hideLoader();
            $saveBtn.prop('disabled', false).html(originalSaveHtml);
        });
}

// Delete Country
function deleteCountry(id) {
    if (!confirm('Are you sure you want to delete this country?')) {
        return;
    }
    
    showLoader();
    
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    $.post('/Countries/Delete', { id: id, __RequestVerificationToken: token })
        .done(function (response) {
            if (response.success) {
                showMessage(response.message, 'success');
                // use returned html if provided, otherwise reload
                if (response.html) {
                    $('#countriesTableContainer').html(response.html);
                } else {
                    loadCountriesList();
                }
            } else {
                showMessage(response.message || 'Failed to delete country.', 'error');
            }
        })
        .fail(function () {
            showMessage('An error occurred while deleting the country.', 'error');
        })
        .always(function () {
            hideLoader();
        });
}

// ===================== LOCATION OPERATIONS =====================

// Load Locations List
function loadLocationsList() {
    showLoader();
    $.get('/Location/GetLocations')
        .done(function (data) {
            if (typeof data === 'string' && data.toLowerCase().indexOf('<form') !== -1 && data.toLowerCase().indexOf('login') !== -1) {
                window.location = '/Account/Login';
                return;
            }
            $('#locationsTableContainer').html(data);
        })
        .fail(function () {
            showMessage('Failed to load locations list.', 'error');
        })
        .always(function () {
            hideLoader();
        });
}

// Open Location Modal
function openLocationModal(id = 0) {
    const isEdit = id > 0;
    const modalTitle = isEdit ? 'Edit Location' : 'Add New Location';
    
    $('#locationModalLabel span').text(modalTitle);
    $('#saveLocationBtn span').text(isEdit ? 'Update Location' : 'Save Location');
    
    // Load form content
    $.get('/Location/AddOrEdit', { id: id }, function (data) {
        $('#locationModalBody').html(data);
        $('#locationModal').modal('show');
        
        // Initialize form validation
        setTimeout(function() {
            initializeFormValidation('#locationForm');
        }, 100);
    }).fail(function () {
        showMessage('Failed to load location form.', 'error');
    });
}

// Save Location
function saveLocation() {
    const form = $('#locationForm');
    const formData = form.serialize();
    const id = form.find('input[name="id"]').val() || 0;
    console.debug('saveLocation submitting form data (serialized):', formData);
    // Ensure anti-forgery token is present in payload (fallback if global setup didn't pick it)
    const token = form.find('input[name="__RequestVerificationToken"]').val();
    let payload = formData;
    if (token && payload.indexOf('__RequestVerificationToken=') === -1) {
        payload = payload ? (payload + '&__RequestVerificationToken=' + encodeURIComponent(token)) : ('__RequestVerificationToken=' + encodeURIComponent(token));
    }
    
    if (typeof form.valid === 'function' && !form.valid()) {
        showMessage('Please correct the validation errors.', 'error');
        return;
    }
    
    // Disable save button during request and show inline spinner
    const $saveBtn = $('#saveLocationBtn');
    const originalSaveHtml = $saveBtn.html();
    $saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Saving...');

    showLoader();

    $.ajax({
        url: '/Location/AddOrEdit',
        method: 'POST',
        data: payload
    })
    .done(function (response) {
        if (response && typeof response === 'object' && response.isValid) {
            showMessage(response.message || 'Saved successfully', 'success');
            $('#locationModal').modal('hide');
            if (response.html) {
                $('#locationsTableContainer').html(response.html);
            } else {
                loadLocationsList();
            }
            return;
        }

        if (typeof response === 'string') {
            try {
                const parsed = JSON.parse(response);
                if (parsed && parsed.isValid) {
                    showMessage(parsed.message || 'Saved successfully', 'success');
                    $('#locationModal').modal('hide');
                    if (parsed.html) $('#locationsTableContainer').html(parsed.html);
                    else loadLocationsList();
                    return;
                }
            } catch (e) {
                $('#locationModalBody').html(response);
                initializeFormValidation('#locationForm');
                return;
            }
        }

        showMessage('Unexpected response from server.', 'warning');
        loadLocationsList();
    })
    .fail(function (xhr) {
        const contentType = xhr.getResponseHeader('Content-Type') || '';
        if (contentType.indexOf('text/html') !== -1 && xhr.responseText) {
            $('#locationModalBody').html(xhr.responseText);
            initializeFormValidation('#locationForm');
            return;
        }

        showMessage('An error occurred while saving the location.', 'error');
    })
    .always(function () {
        hideLoader();
        // restore save button
        $saveBtn.prop('disabled', false).html(originalSaveHtml);
    });
}

// Delete Location
function deleteLocation(id) {
    if (!confirm('Are you sure you want to delete this location?')) {
        return;
    }
    
    showLoader();
    
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    $.post('/Location/Delete', { id: id, __RequestVerificationToken: token })
        .done(function (response) {
            if (response.success) {
                showMessage(response.message, 'success');
                loadLocationsList();
            } else {
                showMessage(response.message || 'Failed to delete location.', 'error');
            }
        })
        .fail(function () {
            showMessage('An error occurred while deleting the location.', 'error');
        })
        .always(function () {
            hideLoader();
        });
}

// ===================== UTILITY FUNCTIONS =====================

// Show message notifications
function showMessage(message, type = 'info') {
    const alertClass = type === 'success' ? 'alert-success' : 
                      type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 'alert-info';
    
    const icon = type === 'success' ? 'check-circle' : 
                type === 'error' ? 'exclamation-triangle' : 
                type === 'warning' ? 'exclamation-circle' : 'info-circle';
    
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show message-alert" role="alert">
            <i class="fas fa-${icon} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    
    // Remove existing alerts (replace left-to-right)
    $('.message-alert').remove();

    // Ensure there is a dedicated messages container fixed to the right, positioned under the navbar
    var $messagesContainer = $('#appMessages');
    if (!$messagesContainer.length) {
        // fixed container on the right under navbar
        $messagesContainer = $(
            '<div id="appMessages" aria-live="polite" aria-atomic="true" ' +
            'style="position:fixed; top:64px; right:20px; width:360px; z-index:2000; pointer-events: none;"></div>'
        );
        $('body').append($messagesContainer);
    }

    // Add new alert: allow clicks inside the alert (pointer-events) and prepend so newest appears at top
    var $alertElem = $(alertHtml);
    $alertElem.css('pointer-events', 'auto');
    $messagesContainer.prepend($alertElem);
    
    // Auto-dismiss after 5 seconds
    setTimeout(function () {
        $('.message-alert').alert('close');
    }, 5000);
}

// Show/Hide loader
function showLoader() {
    if (window.__disableGlobalLoader) return; // loader disabled globally

    if (!$('#globalLoader').length) {
        $('body').append(`
            <div id="globalLoader" class="position-fixed top-0 start-0 w-100 h-100 d-flex justify-content-center align-items-center" 
                 style="background: rgba(0,0,0,0.5); z-index: 9999;">
                <div class="spinner-border text-light" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `);
    }
    $('#globalLoader').show();
    // Clear any previous auto-hide timer
    if (window.__autoHideLoaderTimer) {
        clearTimeout(window.__autoHideLoaderTimer);
    }
    // Safety: auto-hide loader after 30s to avoid stuck state
    window.__autoHideLoaderTimer = setTimeout(function () {
        console.warn('Auto-hiding loader after timeout');
        try { hideLoader(); } catch (e) { console.warn(e); }
    }, 30000);
}

function hideLoader() {
    // If loader globally disabled, ensure any element removed and timers cleared
    if (window.__autoHideLoaderTimer) {
        clearTimeout(window.__autoHideLoaderTimer);
        window.__autoHideLoaderTimer = null;
    }

    if (window.__disableGlobalLoader) {
        var gl = document.getElementById('globalLoader');
        if (gl) gl.remove();
        return;
    }

    $('#globalLoader').hide();
}

// Initialize form validation
function initializeFormValidation(formSelector) {
    const form = $(formSelector);
    if (form.length) {
        form.removeData('validator').removeData('unobtrusiveValidation');
        try {
            // Prefer unobtrusive parse when available
            if (window.jQuery && $.validator && $.validator.unobtrusive && typeof $.validator.unobtrusive.parse === 'function') {
                $.validator.unobtrusive.parse(form);
            } else if (window.jQuery && typeof form.validate === 'function') {
                // Fallback: initialize jQuery validate if unobtrusive is not present
                form.validate();
            } else {
                // Validation scripts not loaded; log a warning but don't throw
                console.warn('Validation scripts not available: jquery.validate and/or jquery.validate.unobtrusive');
            }
        } catch (ex) {
            console.error('Error initializing form validation', ex);
        }
    }
}

// Format date for display
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric', 
        year: 'numeric' 
    });
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, function(m) { return map[m]; });
}

// ===================== ITEM STATUS OPERATIONS =====================

// Load Item Statuses List
function loadItemStatuses() {
    // reuse global loader
    showLoader();
    $.get('/ItemStatuses/GetItemStatuses')
        .done(function (data) {
            if (typeof data === 'string' && data.toLowerCase().indexOf('<form') !== -1 && data.toLowerCase().indexOf('login') !== -1) {
                window.location = '/Account/Login';
                return;
            }
            $('#itemStatusTableContainer').html(data);
        })
        .fail(function () {
            showMessage('Failed to load item statuses list.', 'error');
        })
        .always(function () {
            hideLoader();
        });
}

// Open ItemStatus Modal
function openItemStatusModal(id = 0) {
    const isEdit = id > 0;
    const modalTitle = isEdit ? 'Edit Status' : 'Add New Status';

    $('#itemStatusModalLabel').text(modalTitle);
    // Load form content
    $.get('/ItemStatuses/AddOrEdit', { id: id })
        .done(function (data) {
            $('#itemStatusModalBody').html(data);
            // Bootstrap 5: use native Modal API if available
            try {
                var modalEl = document.getElementById('itemStatusModal');
                if (modalEl) {
                    var bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
                    bsModal.show();
                } else {
                    // fallback to jQuery if available
                    $('#itemStatusModal').modal('show');
                }
            } catch (e) {
                // fallback
                try { $('#itemStatusModal').modal('show'); } catch (e) { console.warn('modal show failed', e); }
            }

            // initialize validation
            setTimeout(function () { initializeFormValidation('#itemStatusForm'); }, 100);
        })
        .fail(function () {
            showMessage('Failed to load status form.', 'error');
        });
}

// Save ItemStatus
function saveItemStatus() {
    const form = $('#itemStatusForm');
    if (!form.length) { 
        showMessage('Form not found', 'error'); 
        return; 
    }

    // Check if form is valid using jQuery validate
    if (!form.valid()) {
        showMessage('Please correct the validation errors.', 'error');
        return false;
    }

    const status = $('#Status').val();
    if (!status || !status.trim()) {
        showMessage('Please enter a status.', 'error');
        return false;
    }

    const $saveBtn = $('#saveItemStatusBtn');
    const originalSaveHtml = $saveBtn.html();
    $saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Saving...');

    showLoader();

    // Prepare the form data
    const formData = new FormData();
    formData.append('Status', status.trim());
    
    // Only include Id if it's an edit operation
    const id = $('#itemStatusForm input[name="Id"]').val();
    if (id && id !== '0') {
        formData.append('Id', id);
    }
    
    // Add anti-forgery token
    const token = $('input[name="__RequestVerificationToken"]').val();
    formData.append('__RequestVerificationToken', token);

    // Make the AJAX call
    $.ajax({
        url: '/ItemStatuses/AddOrEdit',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false
    }).done(function(response) {
        console.log('Save response:', response);
        
        if (response.isValid) {
            showMessage(response.message || 'Status saved successfully', 'success');
            
            // Close the modal
            try {
                var modalInstance = bootstrap.Modal.getInstance(document.getElementById('itemStatusModal'));
                if (modalInstance) modalInstance.hide();
            } catch (e) {
                $('#itemStatusModal').modal('hide');
            }
            
            // Update the table
            if (response.html) {
                $('#itemStatusTableContainer').html(response.html);
            } else {
                loadItemStatusList();
            }
        } else {
            const errorMessage = response.message || 'Failed to save status';
            showMessage(errorMessage, 'error');
        }
    }).fail(function(xhr) {
        console.error('Save failed:', xhr);
        const contentType = xhr.getResponseHeader('Content-Type') || '';
        if (contentType.indexOf('text/html') !== -1 && xhr.responseText) {
            $('#itemStatusModalBody').html(xhr.responseText);
            initializeFormValidation('#itemStatusForm');
        } else {
            const errorMessage = xhr.responseJSON?.message || 'An error occurred while saving. Please try again.';
            showMessage(errorMessage, 'error');
        }
    }).always(function() {
        hideLoader();
        $saveBtn.prop('disabled', false).html(originalSaveHtml);
    });
}
// Load ItemStatus list (if not using the main loadItemStatuses function)
function loadItemStatusList() {
    loadItemStatuses(); // Use the existing function
}

// Document ready function for ItemStatuses
$(document).ready(function() {
    // Load ItemStatuses if on the ItemStatuses page
    if (window.location.pathname.includes('/ItemStatuses')) {
        loadItemStatuses();
    }
    
    // Load Pharmacies if on the Pharmacies page
    if (window.location.pathname.includes('/Pharmacies')) {
        loadPharmacies();
    }
});

// ========================================
// PHARMACY MANAGEMENT FUNCTIONS
// ========================================

// Load pharmacies list
function loadPharmacies() {
    $.ajax({
        url: '/Pharmacies/GetPharmacies',
        type: 'GET',
        success: function (data) {
            $('#pharmacyTableContainer').html(data);
        },
        error: function () {
            showMessage('Error loading pharmacies.', 'error');
        }
    });
}

// Open pharmacy modal (add or edit)
function openPharmacyModal(id = 0) {
    $.ajax({
        url: '/Pharmacies/AddOrEdit',
        type: 'GET',
        data: { id: id },
        success: function (data) {
            $('#pharmacyModal .modal-body').html(data);
            $('#pharmacyModalLabel').text(id === 0 ? 'Add Pharmacy' : 'Edit Pharmacy');
            $('#pharmacyModal').modal('show');
            
            // Initialize form validation after modal content is loaded
            setTimeout(function () { initializeFormValidation('#pharmacyForm'); }, 100);
        },
        error: function () {
            showMessage('Error loading pharmacy form.', 'error');
        }
    });
}

// Save pharmacy (add or edit)
function savePharmacy() {
    var form = $('#pharmacyForm');
    var formData = form.serialize();
    
    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.isValid) {
                $('#pharmacyModal').modal('hide');
                $('#pharmacyTableContainer').html(response.html);
                showMessage(response.message, 'success');
            } else {
                showMessage(response.message || 'Please check your input and try again.', 'error');
            }
        },
        error: function () {
            showMessage('Error saving pharmacy.', 'error');
        }
    });
}

// Delete pharmacy
function deletePharmacy(id) {
    if (confirm('Are you sure you want to delete this pharmacy?')) {
        $.ajax({
            url: '/Pharmacies/Delete',
            type: 'POST',
            data: { 
                id: id,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    $('#pharmacyTableContainer').html(response.html);
                    showMessage(response.message, 'success');
                } else {
                    showMessage(response.message, 'error');
                }
            },
            error: function () {
                showMessage('Error deleting pharmacy.', 'error');
            }
        });
    }
}

// Load Pharmacy list (alias function)
function loadPharmacyList() {
    loadPharmacies(); // Use the existing function
}
