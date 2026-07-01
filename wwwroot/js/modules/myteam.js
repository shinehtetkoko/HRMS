//------------Start MyTeam Filters-----------
/**
 * Attaches asynchronous click event listeners to team identity element targets on page load,
 * firing dynamic data fetch sequences to resolve partial HTML snippets for overlay display modules.
 * 
 * @requires HTML_Elements: .open-popup-btn, #popupContent, #popupContainer
 * @returns {void}
 */
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.open-popup-btn').forEach(button => {
        button.addEventListener('click', function () {
            const empId = this.getAttribute('data-id');

            fetch(`/Team/GetEmployeeDetails?id=${empId}`)
                .then(response => {
                    if (!response.ok) throw new Error("Network response was not ok");
                    return response.text();
                })
                .then(html => {
                    document.getElementById('popupContent').innerHTML = html;
                    document.getElementById('popupContainer').style.display = 'flex';
                })
                .catch(err => console.error("Error loading popup:", err));
        });
    });
});

/**
 * Toggles the visibility layout states of the structural background overlay wrappers,
 * safe-checking existence identifiers to clear modal representations smoothly.
 * 
 * @requires HTML_Elements: #popupContainer
 * @returns {void}
 */
function closePopup() {
    const container = document.getElementById('popupContainer');
    if (container) {
        container.style.display = 'none';
    }
}
//------------End MyTeam Filters-----------