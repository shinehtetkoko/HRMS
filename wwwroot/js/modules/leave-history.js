//----------------------Start Apply Leave----------------------//
/**
 * Opens the Apply Leave Request modal and resets all form fields.
 *
 * @returns {void}
 */
function openApplyLeaveModal(){
    const form = document.getElementById("leaveForm");
    if (form)
    {
        form.reset();
    }
    const fileText = document.getElementById("applySelectedFile");
    if (fileText)
    {
        fileText.innerText = "No file chosen";
        fileText.classList.remove("text-primary");
        fileText.classList.add("text-secondary");
    }
    var myModal = new bootstrap.Modal(document.getElementById('leaveModal'));
    myModal.show();
}
/**
 * Displays the selected attachment file name and validates
 * that the file size does not exceed the maximum allowed limit (5 MB).
 *
 * @param {HTMLInputElement} input - The file input element containing the selected file.
 * @returns {void}
 */
function showApplyFileName(input) {
    var fileLabel = $('#applySelectedFile');
    if (input.files && input.files.length > 0)
    {
        if (input.files[0].size > 5 * 1024 * 1024)
        {
            alert("File size must be less than 5MB.");
            input.value = '';
            fileLabel.text("No file chosen");
        }
        else
        {
            fileLabel.text(input.files[0].name);
        }
    }
}
/**
 * Validates the selected start date.
 * Ensures the start date is not earlier than the current date,
 * prevents timezone-related date issues, and updates the minimum
 * selectable end date when validation succeeds.
 *
 * @returns {boolean|void}
 * Returns true if the date is valid, false if invalid,
 * or void if no start date is selected.
 */
function validateStartDate() {
    let startDateInput = document.getElementById('startDate');
    let errorSpan = document.getElementById('startDateError');
    if (!startDateInput.value)
    {
        return;
    }
    // Extract year, month, and day from the selected date
    let [year, month, day] = startDateInput.value.split('-').map(Number);

    // Create a local date object to avoid timezone-related issues
    let selectedDate = new Date(year, month - 1, day);

    // Get today's date in local time
    let today = new Date();
    today.setHours(0, 0, 0, 0);
    if (selectedDate < today)
    {
        // Display validation error for past dates
        errorSpan.textContent = "Please select a date from today onwards.";
        errorSpan.style.display = 'block';
        startDateInput.value = "";
        startDateInput.classList.add('is-invalid');
        return false;
    }
    else
    {
        // Clear validation error
        errorSpan.style.display = 'none';
        startDateInput.classList.remove('is-invalid');

        // Update the minimum selectable end date
        document.getElementById('endDate').min = startDateInput.value;
        calculateDays();
        return true;
    }
}
/**
 * Calculate Leave Duration and Validate Leave Policy
 *
 * @returns {void}
 */
function calculateDays() {
    const startValue = document.getElementById('startDate').value;
    const endValue = document.getElementById('endDate').value;
    const totalDaysInput = document.getElementById('totalDays');
    const endDateError = document.getElementById('endDateError');
    if (startValue && endValue)
    {
        const startDate = new Date(startValue);
        const endDate = new Date(endValue);
        const diffTime = endDate - startDate;
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
        if (diffDays >= 1)
        {
            totalDaysInput.value = diffDays;
            totalDaysInput.classList.remove('is-invalid');
            if (endDateError)
            {
                endDateError.style.display = 'none';
            }
        }
        else
        {
            totalDaysInput.value = "";
            document.getElementById('endDate').value = "";
            if (endDateError)
            {
                endDateError.innerText = "End date must be after start date.";
                endDateError.style.display = 'block';
            }
        }
    }
    const selectedTypeId = document.getElementById('leaveTypeId').value;
    const totalDays = parseInt(totalDaysInput.value);
    // Retrieve the selected leave policy
    const policy = policies.find(p => p.leave_Type_Id == selectedTypeId);
    if (policy && totalDays > policy.max_Days)
    {
        totalDaysInput.value = "";
    }
}
/**
 * Validates whether the requested leave days are within the employee's
 * available leave balance for the selected leave type.
 *
 * @param {number} requestedDays - The number of leave days requested by the employee.
 * @returns {boolean} Returns true if sufficient balance is available; otherwise, false.
 */
function checkBalance(requestedDays) {
    const jsonString = document.getElementById('userBalancesJson').value;
    const userBalances = JSON.parse(jsonString);
    const selectedTypeId = document.getElementById('leaveTypeId').value;
    const balanceRecord = userBalances.find(b => b.leave_Type_Id == selectedTypeId);
    if (balanceRecord && requestedDays > balanceRecord.remaining_Days)
    {
        alert("Insufficient balance! You have only " + balanceRecord.remaining_Days + " days.");
        return false;
    }
    return true;
}
/**
 * Validate Leave Policy Limits
 *
 * @returns {void}
 */
function checkLeavePolicy() {
    const leaveTypeElement = document.getElementById('leaveTypeId');
    const totalDaysInput = document.getElementById("modalTotalDays");
    const selectedTypeId = leaveTypeElement.value;
    const totalDays = parseInt(totalDaysInput.value);
    // Ensure required fields are available before validation
    if (!leaveTypeElement || !totalDaysInput.value)
    {
        return;
    }
    // Retrieve the selected leave policy
    const policy = policies.find(p => p.leave_Type_Id == selectedTypeId);
    if (policy && totalDays > policy.max_Days)
    {
        alert("Maximum leave duration is " + policy.max_Days + " days.");
        totalDaysInput.value = "";  // Clear the invalid leave duration value
    }
}
/**
 * Configure Medical Leave Attachment Requirements
 *
 * @returns {void}
 */
$(document).ready(function () {
    var attachmentWrapper = $('#Attachment').closest('div');
    attachmentWrapper.css('pointer-events', 'none').css('opacity', '0.5');
    $('#leaveTypeId').on('change', function () {
        var selectedText = $("#leaveTypeId option:selected").text().trim().toUpperCase();
        var attachmentWrapper = $('#Attachment').closest('div');
        if (selectedText === 'MEDICAL')
        {
            attachmentWrapper.css('pointer-events', 'auto').css('opacity', '1');
            $('#Attachment').prop('required', true);
        }
        else
        {
            attachmentWrapper.css('pointer-events', 'none').css('opacity', '0.5');
            $('#Attachment').val('');
            $('#Attachment').prop('required', false);
            $('#applySelectedFile').text('No file chosen');
        }
    });
});
/**
 * Submits a new leave request to the server.
 * Collects form data, sends the request via AJAX,
 * and displays success or error feedback to the user.
 *
 * @returns {void}
 */
function handleFormSubmission() {
    let form = document.getElementById('leaveForm');
    let feedback = document.getElementById('formFeedback');
    let formData = new FormData(form);
    fetch('/Leave/Apply', { method: 'POST', body: formData }).then(response => response.json()).then(data => {
        feedback.style.display = 'block';
        if (data.success)
        {
            feedback.className = "mt-3 p-2 text-center rounded-2 bg-success text-white";
            feedback.innerText = data.message;
            setTimeout(() =>
            {
                $('#leaveModal').modal('hide');
                form.reset();
                feedback.style.display = 'none';
            }, 1000);
        }
        else
        {
            feedback.className = "mt-3 p-2 text-center rounded-2 bg-danger text-white";
            feedback.innerText = "Error: " + data.message;
        }
    })
        .catch(error => {
            feedback.style.display = 'block';
            feedback.className = "mt-3 p-2 text-center rounded-2 bg-danger text-white";
            feedback.innerText = "Something went wrong on our end.";
        });
}
//-----------------------End Apply Leave-----------------------//

//-------------------Start Leave History-------------------//
// Tracks the currently selected page number
let currentPage = 1;
/**
 * Initializes leave history page events and manages
 * data loading, filtering, reset functionality, and pagination state.
 *
 * @returns {void}
 */
document.addEventListener("DOMContentLoaded", function () {
    // Load the first page of leave history data on page initialization
    loadData(1);
    // Handle filter button click event
    document.getElementById('filterBtn').addEventListener('click', function () {
        // Reset pagination and load filtered data from page 1
        currentPage = 1;
        loadData(currentPage);
    });
    // Handle reset button click event
    document.getElementById('resetBtn').addEventListener('click', function () {
        // Restore the default month selection
        const defaultMonth = document.getElementById('dataContainer')?.getAttribute('data-default-month');
        if (defaultMonth)
        {
            document.getElementById('monthSelect').value = defaultMonth;
        }
        // Reload data while preserving the current page number
        loadData(currentPage);
    });
});
/**
 * Loads leave history data based on the selected filters,
 * updates the table, pagination controls, and result summary.
 *
 * @param {number} page - The page number to load.
 * @returns {void}
 */
window.loadData = function (page) {
    // Retrieve filter and display elements
    const monthSelect = document.getElementById('monthSelect');
    const yearSelect = document.getElementById('yearSelect');
    const resultText = document.getElementById('resultText');
    const monthValue = monthSelect.value;
    const monthText = monthSelect.options[monthSelect.selectedIndex].text;
    const yearText = yearSelect.getAttribute('data-current-year');
    // Request filtered leave history data from the server
    fetch(`/Leave/GetFilteredHistory?month=${monthValue}&year=${yearText}&page=${page}`).then(response => {
        if (!response.ok)
        {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
        .then(data => {
            if (data && data.items)
            {
                // Store the current page number
                currentPage = data.currentPage;
                // Render leave history records in the table
                renderTable(data.items);
                // Generate pagination controls
                renderPagination(data.totalPages, data.currentPage);
                // Update record count summary text
                updateResultText(data.totalCount, data.currentPage);
                // Update selected month and year display
                if (resultText) {
                    const displayMonth = (monthValue === "") ? "All Months" : monthText;
                    resultText.innerText = `${displayMonth} ${yearText}`;
                }
            }
        })
        .catch(err => {
            console.error("Fetch Error:", err);
            // Display an error message if data loading fails
            const tbody = document.getElementById('leaveTableBody');
            if (tbody)
            {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center">Error loading data.</td></tr>';
            }
        });
};
/**
 * Renders leave history records into the table and
 * generates action buttons based on leave request status.
 *
 * @param {Array<Object>} items - Collection of leave history records.
 * @returns {void}
 */
function renderTable(items) {
    // Get the table body element
    const tbody = document.getElementById('leaveTableBody');
    if (!tbody)
    {
        return;
    }
    tbody.innerHTML = '';
    items.forEach(item => {
        const start = new Date(item.startDate).toLocaleDateString('en-US', { month: 'short', day: '2-digit' });
        const end = new Date(item.endDate).toLocaleDateString('en-US', { month: 'short', day: '2-digit' });
        // Create the default View action button
        let actionButtons = `
            <label class="text-primary fw-medium" style="cursor: pointer;" onclick="openLeaveModal('view', '${item.leaveRequestId}', '${item.employeeId}', '${item.employeeName}', '${item.leaveType}', '${item.startDate}', '${item.endDate}', '${item.totalDays}', '${item.reason}', '${item.attachment || ''}')"> 
            View
            </label>`;
        // Display Update and Delete actions only for pending requests
        if (item.status === 'Pending') {
            actionButtons += `
                <span class="text-black-50 mx-2">|</span>
                <label class="text-primary fw-medium" style="cursor: pointer;" onclick="openLeaveModal('update', '${item.leaveRequestId}', '${item.employeeId}', '${item.employeeName}', '${item.leaveType}', '${item.startDate}', '${item.endDate}', '${item.totalDays}', '${item.reason}', '${item.attachment || ''}')">
                Update
                </label>
                <span class="text-black-50 mx-2">|</span>
                <label class="text-danger fw-medium" style="cursor: pointer;"  onclick="confirmDeleteRow(this, '${item.leaveRequestId}')"> 
                Delete
                </label>
            `;
        }
        // Append a new leave history row to the table
        tbody.innerHTML += `
                <tr class="border-bottom">
                <td class="py-3 px-4 fw-medium">${item.employeeId}</td>
                <td class="py-3 text-secondary">${item.employeeName}</td>
                <td class="py-3 text-secondary">${item.departmentName}</td>
                <td class="py-3 text-secondary">${item.leaveType}</td>
                <td class="py-3 text-secondary" style="white-space: nowrap;">${start} - ${end}</td>
                <td class="py-3 text-secondary">${item.totalDays}</td>
                <td class="py-3"><span class="badge rounded-pill ${getBadgeClass(item.status)} px-3 py-2 fw-semibold">${item.status}</span></td>
                <td>
                    <div class="d-flex align-items-center">
                        ${actionButtons}
                    </div>
                </td>
        <tr>`;
    });
}
/**
 * Generates and renders pagination controls for the leave history table.
 *
 * @param {number} totalPages - Total number of available pages.
 * @param {number} currentPage - Currently active page number.
 * @returns {void}
 */
function renderPagination(totalPages, currentPage) {
    // Get the pagination container element
    const paginationUl = document.getElementById('paginationControls');
    if (!paginationUl)
    {
        return;
    }
    paginationUl.innerHTML = '';
    // Render the Previous page button
    paginationUl.innerHTML += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}"><a class="page-link" href="#" onclick="event.preventDefault(); loadData(${currentPage - 1})">Previous</a></li>`;
    for (let i = 1; i <= totalPages; i++) {
        paginationUl.innerHTML += `<li class="page-item ${i === currentPage ? 'active' : ''}"> <a class="page-link" href="#" onclick="event.preventDefault(); loadData(${i})">${i}</a></li>`;
    }
    paginationUl.innerHTML += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}"> <a class="page-link" href="#" onclick="event.preventDefault(); loadData(${currentPage + 1})">Next</a></li>`;
}
/**
 * Returns the appropriate CSS badge class based on
 * the leave request status.
 *
 * @param {string} status - The current leave request status.
 * @returns {string} CSS class name used for badge styling.
 */
function getBadgeClass(status) {
    if (status === "Pending")
    {
        return "custom-badge badge-pending-bright";
    }
    if (status === "Approved")
    {
        return "custom-badge badge-approved-bright";
    }
    if (status === "Rejected")
    {
        return "custom-badge badge-rejected-bright";
    }
    return "bg-secondary-subtle text-secondary";
}
/**
 * Updates the leave history summary text by displaying
 * the current record range and total record count.
 *
 * @param {number} totalCount - Total number of records.
 * @param {number} currentPage - Currently active page number.
 * @returns {void}
 */
function updateResultText(totalCount, currentPage) {
    const resultText = document.getElementById('resultTextnew');
    if (!resultText)
    {
        console.error("resultText element not found");
        return;
    }
    const pageSize = 10;
    if (totalCount === 0)
    {
        resultTextnew.innerText = "Showing 0 records";
    }
    else
    {
        const start = (currentPage - 1) * pageSize + 1;
        const end = Math.min(currentPage * pageSize, totalCount);
        resultTextnew.innerText = `Showing ${start} to ${end} of ${totalCount} records`;
    }
}
//--------------------End Leave History--------------------//

//----------------Start Leave Details----------------//
/**
 * Global collection of leave policies.
 */
let policies = [];
/**
 * Retrieves leave policies from the server when the page loads.
 */
fetch('/Leave/GetLeavePolicies').then(res => res.json()).then(data => { policies = data; }).catch(err => console.error("Error fetching policies:", err));
/**
 * Populates the Leave Type dropdown with available leave policies.
 * Disables leave types that have no remaining balance.
 *
 * @returns {void}
 */
function populateLeaveTypes() {
    const leaveSelect = document.getElementById("modalLeaveType");
    if (!leaveSelect)
    {
        return;
    }
    leaveSelect.innerHTML = '<option value="">Please select a leave type</option>';
    const seenIds = new Set();
    policies.forEach(policy => {
        const leaveId = policy.leave_Type_Id;
        const leaveName = policy.leaveType ? policy.leaveType.leave_Name : "Unknown";
        const isOutOfBalance = policy.balance <= 0;
        if (!seenIds.has(leaveId))
        {
            const option = document.createElement("option");
            option.value = isOutOfBalance ? "" : leaveId;
            option.disabled = isOutOfBalance;
            option.setAttribute("data-maxdays", policy.max_Days);
            option.text = leaveName + (isOutOfBalance ? " (Out of balance)" : "");
            leaveSelect.appendChild(option);
            seenIds.add(leaveId);
        }
    });
}
/**
 * Controls attachment requirements for Medical Leave requests.
 * Shows or hides the attachment upload section based on the selected leave type.
 * Makes the attachment mandatory when Medical Leave is selected in Update mode.
 *
 * @param {string} mode - The current modal mode (view or update).
 * @returns {void}
 */
function checkMedicalAttachmentRequirement(mode) {
    const leaveTypeSelect = document.getElementById("modalLeaveType");
    const attachmentContainer = document.getElementById("fileUploadContainer");
    const fileInput = document.getElementById("modalAttachmentFile");
    const fileNameDisplay = document.getElementById("modalSelectedFile");
    if (mode !== 'update' || !attachmentContainer)
    {
        if (attachmentContainer)
        {
            attachmentContainer.style.setProperty('display', 'none', 'important');
        }
        if (fileInput)
        {
            fileInput.required = false;
        }
        return;
    }
    if (!leaveTypeSelect)
    {
        return;
    }
    const selectedText = leaveTypeSelect.options[leaveTypeSelect.selectedIndex].text.toLowerCase();
    if (selectedText.includes("medical"))
    {
        attachmentContainer.style.setProperty('display', 'flex', 'important');
        if (fileInput)
        {
            fileInput.required = true;
        }
    }
    else
    {
        attachmentContainer.style.setProperty('display', 'none', 'important');
        if (fileInput)
        {
            fileInput.required = false;
            fileInput.value = '';
        }
        if (fileNameDisplay)
        {
            fileNameDisplay.innerText = "No file chosen";
        }
        const existingLink
            = document.getElementById("existingAttachmentLink");
        if (existingLink)
        {
            existingLink.style.display = 'none';
        }
    }
}
/**
 * Validates the leave request form before submission.
 * Ensures that Medical Leave requests include either a new attachment
 * or an existing medical certificate.
 *
 * @returns {boolean} Returns true if validation passes; otherwise false.
 */
function validateLeaveForm() {
    const leaveTypeSelect = document.getElementById("modalLeaveType");
    const fileInput = document.getElementById("modalAttachmentFile");
    const existingLink = document.getElementById("existingAttachmentLink");

    const selectedText = leaveTypeSelect.options[leaveTypeSelect.selectedIndex].text.toLowerCase();
    // Display an error if Medical leave has no new or existing attachment
    if (selectedText.includes("medical"))
    {
        const hasNewFile = fileInput.files.length > 0;
        const hasExistingFile = existingLink.querySelector('a') !== null;

        if (!hasNewFile && !hasExistingFile)
        {
            alert("Please attach your medical certificate to process your leave request.");
            return false;
        }
    }
    return true;
}
/**
 * Opens the Leave Details modal.
 *
 * @param {string} mode - Modal mode (view or update).
 * @param {number|string} id - Leave request ID.
 * @param {string} empId - Employee ID.
 * @param {string} empName - Employee name.
 * @param {string} leaveType - Leave type name.
 * @param {string} startDate - Leave start date.
 * @param {string} endDate - Leave end date.
 * @param {number|string} totalDays - Total leave days.
 * @param {string} reason - Leave request reason.
 * @param {string|null} attachment - Attachment file name.
 * @returns {void}
 */
function openLeaveModal(mode, id, empId, empName, leaveType, startDate, endDate, totalDays, reason, attachment) {
    var myModal = new bootstrap.Modal(document.getElementById('leaveDetailsModal'));
    const modalLabel = document.getElementById('leaveDetailsModalLabel');
    const modalSubTitle = document.getElementById('modalSubTitle');
    if (mode === 'update')
    {
        modalLabel.innerText = "Update Leave";
        modalSubTitle.innerText = "Modify your pending request parameters below.";
    }
    else
    {
        modalLabel.innerText = "View Leave Details";
        modalSubTitle.innerText = "Review your submitted leave request parameters.";
    }
    myModal.show();
    populateLeaveTypes();
    setTimeout(() => {
        // Basic fields
        document.getElementById("modalLeaveRequestId").value = id;
        document.getElementById("modalEmpId").value = empId;
        document.getElementById("modalEmpName").value = empName;
        document.getElementById("modalReason").value = reason;
        document.getElementById("modalTotalDays").value = totalDays;
        document.getElementById("modalReason").readOnly = (mode !== 'update');
        // Leave Type Setup
        const leaveTypeSelect = document.getElementById("modalLeaveType");
        if (leaveTypeSelect)
        {
            leaveTypeSelect.disabled = (mode !== 'update');
            const optionToSelect = Array.from(leaveTypeSelect.options).find(o => o.text.toLowerCase().includes(leaveType.toLowerCase()));
            leaveTypeSelect.value = optionToSelect ? optionToSelect.value : "";
            // Revalidate attachment requirements when the Leave Type changes
            leaveTypeSelect.onchange = function () { checkMedicalAttachmentRequirement(mode); };
        }
        // Attachment Logic
        const attachmentLinkDiv = document.getElementById("existingAttachmentLink");
        attachmentLinkDiv.innerHTML = "";
        const cleanAttachment = (attachment === "null" || attachment === "" || attachment === null) ? null : attachment;
        if (cleanAttachment)
        {
            const link = document.createElement("a");
            link.href = "/Leave/DownloadAttachment?fileName=" + encodeURIComponent(cleanAttachment.trim());
            link.target = "_blank";
            link.className = "btn btn-sm btn-primary text-white mb-2";
            link.innerText = "Download Attachment";
            attachmentLinkDiv.appendChild(link);
        }
        else if (mode !== 'update')
        {
            attachmentLinkDiv.innerHTML = '<span class="text-muted small">No attachment found.</span>';
        }
        // Check medical leave attachment requirements when the modal is opened
        checkMedicalAttachmentRequirement(mode);
        // Date Logic
        const isReadonly = (mode !== 'update');
        const formattedStart = (startDate && typeof startDate === 'string') ? startDate.split('T')[0].split(' ')[0] : "";
        const formattedEnd = (endDate && typeof endDate === 'string') ? endDate.split('T')[0].split(' ')[0] : "";
        document.getElementById("startDateContainer").innerHTML = `<input type="date" id="inputStartDate" class="form-control" value="${formattedStart}" ${isReadonly ? 'readonly' : ''} onchange="updateEndDateMin(); calculateDaysForModal();">`;
        document.getElementById("endDateContainer").innerHTML = `<input type="date" id="inputEndDate" class="form-control" value="${formattedEnd}" ${isReadonly ? 'readonly' : ''} onchange="calculateDaysForModal();">`;
        if (mode === 'update')
        {
            updateEndDateMin();
        }
        const submitBtn = document.getElementById("modalSubmitBtn");
        if (submitBtn)
        {
            if (mode === 'update')
            {
                submitBtn.style.display = 'inline-block';
                submitBtn.onclick = function () {
                    if (validateLeaveForm())
                    {
                        handleSaveChanges();
                    }
                };
            }
            else
            {
                submitBtn.style.display = 'none';
            }
        }
    }, 200);
}
/**
 * Sets the minimum selectable End Date based on the selected Start Date.
 * Prevents users from selecting an End Date earlier than the Start Date.
 *
 * @returns { void}
 */
function updateEndDateMin() {
    const startInput = document.getElementById("inputStartDate");
    const endInput = document.getElementById("inputEndDate");
    if (startInput && endInput && startInput.value)
    {
        endInput.min = startInput.value;
    }
}
/**
 * Updates the selected file name in the modal.
 *
 * @param {HTMLInputElement} input - Selected file input.
 * @returns {void}
 */
function showModalFileName(input) {
    const fileNameDisplay = document.getElementById("modalSelectedFile");
    if (input.files && input.files.length > 0)
    {
        fileNameDisplay.innerText = input.files[0].name;
    }
    else
    {
        fileNameDisplay.innerText = "No file chosen";
    }
}

/**
 * Calculate Leave Duration and Validate Leave Policy
 *
 * @returns {void}
 */
function calculateDaysForModal() {
    const startInput = document.getElementById("inputStartDate");
    const endInput = document.getElementById("inputEndDate");
    const totalDaysInput = document.getElementById("modalTotalDays");
    const leaveTypeElement = document.getElementById('modalLeaveType');
    // Parse date values without timezone conversion
    const startParts = startInput.value.split('-');
    const endParts = endInput.value.split('-');
    // Create local date objects to avoid timezone-related issues
    const d1 = new Date(startParts[0], startParts[1] - 1, startParts[2]);
    const d2 = new Date(endParts[0], endParts[1] - 1, endParts[2]);
    const diffTime = d2.getTime() - d1.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    // Validate that the End Date is not earlier than the Start Date
    if (d2 < d1)
    {
        alert("Please ensure the End Date is not before the Start Date.");
        endInput.value = "";
        totalDaysInput.value = "";
        return;
    }
    const policy = policies.find(p => p.leave_Type_Id == leaveTypeElement.value);
    if (policy)
    {
        if (diffDays > policy.max_Days)
        {
            alert(`Leave request cannot be more than ${policy.max_Days} days. `);
            endInput.value = "";
            totalDaysInput.value = "";
            return;
        }
        // Validate available leave balance
        if (diffDays > policy.balance)
        {
            alert(`Leave exceeds available balance of (${policy.balance} days.`);
            endInput.value = "";
            totalDaysInput.value = "";
            return;
        }
    }
    totalDaysInput.value = diffDays;
}
/**
 * Reset Leave Date Selection
 *
 * @returns {void}
 */
function clearEndDate() {
    const startInput = document.getElementById("inputStartDate");
    const endInput = document.getElementById("inputEndDate");
    const totalDaysInput = document.getElementById("modalTotalDays");
    endInput.value = "";
    totalDaysInput.value = "";
    if (startInput.value)
    {
        endInput.min = startInput.value;
    }
}
/**
 * Updates an existing leave request.
 * Validates input data, checks leave policy constraints,
 * uploads attachment files, and submits the updated request to the server.
 *
 * @async
 * @returns {Promise<void>}
 */
async function handleSaveChanges() {
    // Retrieve form values from the modal
    const leaveTypeId = document.getElementById("modalLeaveType").value;
    const totalDaysInput = document.getElementById("modalTotalDays").value;
    const totalDays = parseInt(totalDaysInput);
    const requestId = document.getElementById("modalLeaveRequestId").value;
    const startDate = document.getElementById("inputStartDate").value;
    const endDate = document.getElementById("inputEndDate").value;
    const reason = document.getElementById("modalReason").value;
    // Retrieve the selected leave policy
    const policy = policies.find(p => p.leave_Type_Id == leaveTypeId);
    // Perform input validation checks
    if (!leaveTypeId)
    {
        alert("Error: Please select a leave type.");
        return;
    }
    if (isNaN(totalDays) || totalDays <= 0)
    {
        alert("Error: Please select the dates correctly.");
        return;
    }
    if (policy)
    {
        const currentRequestDays = parseInt(document.getElementById("modalTotalDays").value) || 0;
        const availableBalance = policy.balance + currentRequestDays;
        if (totalDays > availableBalance)
        {
            alert(`"Error: Insufficient leave balance. (Remaining: ${policy.balance} days)`);
            return;
        }
    }
    // Build form data for submission
    const formData = new FormData();
    formData.append("LeaveRequestId", requestId);
    formData.append("LeaveTypeId", leaveTypeId);
    formData.append("StartDate", startDate);
    formData.append("EndDate", endDate);
    formData.append("TotalDays", totalDays);
    formData.append("Reason", reason);
    // Attach the selected file if available
    const fileInput = document.getElementById("modalAttachmentFile");
    if (fileInput && fileInput.files.length > 0)
    {
        formData.append("Attachment", fileInput.files[0]);
    }
    // Submit the updated leave request to the server
    try {
        const response = await fetch('/Leave/Update', {
            method: 'POST',
            body: formData
        });
        const result = await response.json();
        if (response.ok && result.success)
        {
            alert("Completed successfully.");
            location.reload();
        }
        else
        {
            alert("Error: " + (result.message || "Update unsuccessful."));
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Server error: Unable to complete the request.");
    }
}
/**
 * Delete Leave Request
 *
 * @param {HTMLElement} element - Delete action element.
 * @param {number|string} id - Leave request ID.
 * @returns {void}
 */
function confirmDeleteRow(element, id) {
    if (id === undefined || id === null || id === '')
    {
        alert("ID not found. Please provide complete information.");
        return;
    }
    if (confirm("Are you certain you want to cancel this leave request?")) {
        $.ajax({
            url: '/Leave/DeleteLeave', type: 'POST', data: { id: id }, success: function (response) {
                if (response.success)
                {
                    alert(response.message);
                    location.reload();
                }
                else
                {
                    alert(response.message);
                }
            },
            error: function (xhr, status, error) { alert("Unable to connect to the server."); }
        });
    }
}
/**
 * Close Active Modals and Alerts
 *
 * @returns {void}
 */
function closeAllModals() {
    const alertOverlay = document.getElementById("customSuccessAlert");
    if (alertOverlay)
    {
        alertOverlay.classList.add("d-none");
    }
    const myModalEl = document.getElementById('leaveDetailsModal');
    const modal = bootstrap.Modal.getInstance(myModalEl);
    if (modal)
    {
        modal.hide();
    }
    // location.reload();Refresh
}
//-----------------End Leave Details-----------------//