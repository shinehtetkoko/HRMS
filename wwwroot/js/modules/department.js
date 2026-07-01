//-----------------------Start Department Setup----------------------------//
/**
 * Global initialization for Department Modals.
 * Sets up Bootstrap modal instances when the DOM is fully loaded.
 */
document.addEventListener('DOMContentLoaded', function () {
    const createModalEl = document.getElementById('createModal');
    if (createModalEl)
    {
        window.createModal = new bootstrap.Modal(createModalEl);
    }
});

/**
 * Opens the 'Create Department' modal and auto-generates the next ID.
 */
function openDepartmentModal() {
    if (window.createModal)
    {
        const count = document.querySelectorAll('#dept-table-body tr:not(#no-data-row)').length + 1;
        document.getElementById('input-dept-id').value = "DEPT-" + String(count).padStart(3, '0');
        window.createModal.show();
    }
}

/**
 * Hides the 'Create Department' modal.
 */
function closeDepartmentModal() {
    if (window.createModal)
    { 
        window.createModal.hide();
    }
}

/**
 *  Saves new department data to the server.
 * @param {Event} event - The DOM event triggered by the form submission.
 * @param {string} redirectUrl - The URL to redirect to after successful save.
 */
async function saveDepartmentData(event, redirectUrl) {
    event.preventDefault();
    const deptName = document.getElementById('input-dept-name').value;

    if (!deptName)
    {
        alert("Please enter a department name.");
        return;
    }

    try
    {
            const response = await fetch('/DeptSetup/CreateDepartment',
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ DepartmentName: deptName })
            });

             const result = await response.json();
        if (result.success)
        {
            alert("Data saved successfully.");
            window.location.href = redirectUrl; // HTML မှ ပို့ပေးလိုက်သော URL ကို သုံးပါ
        }
        else
        {
            alert("Error saving data!");
        }
    } catch (error)
    {
        console.error("Error:", error);
    }
}

/**
 * Opens the 'Edit Department' modal and populates fields with existing data.
 * @param {number|string} id - The database ID of the department.
 * @param {string} name - The name of the department.
 */
function openEditModal(id, name) {
    document.getElementById('edit-dept-id').value = id;
    let formattedId = "DEPT-" + id.toString().padStart(3, '0');
    document.getElementById('edit-dept-id-display').value = formattedId;
    document.getElementById('edit-dept-name').value = name;
    var myModal = new bootstrap.Modal(document.getElementById('editModal'));
    myModal.show();
}

/**
 * Prompts user for confirmation before deleting a department.
 * @param {number|string} id - The ID of the department to delete.
 */
function confirmDelete(id) {
    if (confirm("Are you sure you want to proceed?"))
    {
        window.location.href = '/DeptSetup/Delete/' + id;
    }
}
//-------------------------End Department Setup-------------------------------//

