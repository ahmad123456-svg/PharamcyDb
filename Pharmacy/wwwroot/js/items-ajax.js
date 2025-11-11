// Items AJ        .fail(function() {
            showMessage('Error loading form', 'error'); Functions

function showAddEditModal(id) {
    const title = id === 0 ? 'Add New Item' : 'Edit Item';
    $('#modalTitle').text(title);
    
    $.get('/Items/AddOrEdit', { id: id })
        .done(function (data) {
            $('#itemModalBody').html(data);
            $('#itemModal').modal('show');
        })
        .fail(function () {
            showMessage('Error loading form', 'error');
        });
}

function saveItem() {
    const form = $('#itemForm');
    const formData = form.serialize();
    
    $.ajax({
        url: '/Items/AddOrEdit',
        type: 'POST',
        data: formData,
        success: function (result) {
            if (result.success) {
                $('#itemModal').modal('hide');
                showMessage(result.message, 'success');
                refreshItemsTable();
            } else {
                // If there are validation errors, update the modal content
                if (typeof result === 'string') {
                    $('#itemModalBody').html(result);
                } else {
                    showMessage(result.message || 'Error saving item', 'error');
                }
            }
        },
        error: function () {
            showMessage('Error occurred while saving', 'error');
        }
    });
}

function deleteItem(id, itemName) {
    if (confirm(`Are you sure you want to delete "${itemName}"?`)) {
        $.ajax({
            url: '/Items/Delete',
            type: 'POST',
            data: { id: id },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    showMessage(result.message, 'success');
                    refreshItemsTable();
                } else {
                    showMessage(result.message || 'Error deleting item', 'error');
                }
            },
            error: function () {
                showMessage('Error occurred while deleting', 'error');
            }
        });
    }
}

function refreshItemsTable() {
    $.post('/Items/GetAllItems')
        .done(function (data) {
            $('#itemsTableContainer').html(data);
        })
        .fail(function () {
            showMessage('Error refreshing items table', 'error');
        });
}

// Notification helper function
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

// Initialize form validation when document is ready
$(document).ready(function() {
    // Any initialization code can go here
});

function initializeFormValidation(formSelector) {
    $(formSelector).find('input, select, textarea').on('blur change', function() {
        const field = $(this);
        const value = field.val();
        const isRequired = field.prop('required');
        
        if (isRequired && (!value || value.trim() === '')) {
            field.addClass('is-invalid');
        } else {
            field.removeClass('is-invalid').addClass('is-valid');
        }
    });
}