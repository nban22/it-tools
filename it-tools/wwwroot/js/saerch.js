// Global search functions
window.searchFunctions = {
    // Function to scroll to selected item in search results
    scrollToSelectedItem: function(selectedIndex) {
        try {
            const container = document.querySelector('.max-h-96');
            if (!container) return;
            
            const selectedItem = container.children[selectedIndex];
            if (!selectedItem) return;
            
            const containerRect = container.getBoundingClientRect();
            const selectedRect = selectedItem.getBoundingClientRect();
            
            if (selectedRect.bottom > containerRect.bottom) {
                selectedItem.scrollIntoView(false);
            } else if (selectedRect.top < containerRect.top) {
                selectedItem.scrollIntoView(true);
            }
        } catch (e) {
            console.error("Error scrolling to item:", e);
        }
    }
};