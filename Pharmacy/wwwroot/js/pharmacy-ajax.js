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

// ===================== COUNTRY OPERATIONS =====================

// Load Countries List
function loadCountriesList() {
    showLoader();
    $.get('/Countries/GetCountries')
        .done(function (data) {
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
    $('#saveCountryBtn span').text(isEdit ? 'Update Country' : 'Save Country');

    // Load form content
    $.get('/Countries/AddOrEdit', { id: id }, function (data) {
        $('#countryModalBody').html(data);
        $('#countryModal').modal('show');

        // Initialize form validation
        setTimeout(function() {
            initializeFormValidation('#countryForm');
        }, 100);
    }).fail(function () {
        showMessage('Failed to load country form.', 'error');
    });
}

// Save Country
function saveCountry() {
    const form = $('#countryForm');
    const formData = form.serialize();
    const id = form.find('input[name="id"]').val() || 0;

    // If form plugin present use its validation otherwise skip
    if (typeof form.valid === 'function' && !form.valid()) {
        showMessage('Please correct the validation errors.', 'error');
        return;
    }

    // Disable save button to prevent duplicate clicks and show inline spinner
    const $saveBtn = $('#saveCountryBtn');
    const originalSaveHtml = $saveBtn.html();
    $saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Saving...');

    showLoader();

    $.ajax({
        url: '/Countries/AddOrEdit',
        method: 'POST',
        data: formData
    })
    .done(function (response) {
        console.debug('saveLocation done response:', response);
        console.debug('saveCountry done response:', response);
        // If server returned JSON (object) with isValid flag
        if (response && typeof response === 'object' && response.isValid) {
            showMessage(response.message || 'Saved successfully', 'success');
            $('#countryModal').modal('hide');
            if (response.html) {
                $('#countriesTableContainer').html(response.html);
            } else {
                // fallback: reload list
                loadCountriesList();
            }
            return;
        }

        // If server returned HTML (validation errors or entire partial)
        if (typeof response === 'string') {
            try {
                const parsed = JSON.parse(response);
                if (parsed && parsed.isValid) {
                    showMessage(parsed.message || 'Saved successfully', 'success');
                    $('#countryModal').modal('hide');
                    if (parsed.html) $('#countriesTableContainer').html(parsed.html);
                    else loadCountriesList();
                    return;
                }
            } catch (e) {
                // not JSON - treat as HTML fragment (validation errors)
                $('#countryModalBody').html(response);
                initializeFormValidation('#countryForm');
                return;
            }
        }

        // Unknown response - show message and reload list to be safe
        showMessage('Unexpected response from server.', 'warning');
        loadCountriesList();
    })
    .fail(function (xhr) {
        console.debug('saveLocation failed xhr:', xhr);
        console.debug('saveCountry failed xhr:', xhr);
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
        // restore save button
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
        $.validator.unobtrusive.parse(form);
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