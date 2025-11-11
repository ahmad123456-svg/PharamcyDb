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
});