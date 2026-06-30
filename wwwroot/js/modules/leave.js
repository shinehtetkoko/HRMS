let currentPage = 1, pageSize = 10;

/** 
 * Helper function to select DOM elements by ID
 * @param {string} id 
 * @returns {HTMLElement|null}
 */
const $ = id => document.getElementById(id);

document.addEventListener("DOMContentLoaded", () => {
    initFilters();
    setupEventListeners();
    loadLeaveData();
});

/**
 * Initializes global event listeners for UI interactions
 */
function setupEventListeners() {
    $("viewModeSelect")?.addEventListener("change", function () {
        $("leaveHistoryFilters")?.classList.toggle("d-none", this.value !== "LeaveHistory");
        resetPageAndReload();
    });
    document.querySelector(".filter-btn")?.addEventListener("click", resetPageAndReload);
    document.querySelector(".reset-btn")?.addEventListener("click", () => {
        if ($("monthSelect")) $("monthSelect").value = new Date().getMonth() + 1;
        resetPageAndReload();
    });
    $("exportExcelBtn")?.addEventListener("click", () => {
        const mode = $("viewModeSelect").value;
        let url = `/Leave/ExportLeaveToExcel?mode=${encodeURIComponent(mode)}`;
        if (mode === "LeaveHistory" && $("monthSelect") && $("yearSelect"))
        {
            url += `&month=${$("monthSelect").value}&year=${$("yearSelect").value}`;
        }
        window.location.href = url;
    });
}
/**
 * Resets the pagination and reloads the table data
 */
const resetPageAndReload = () => {
    currentPage = 1; updateBanner(); loadLeaveData();
};
/**
 * Sets default values for filter inputs on page load
 */
function initFilters() {
    if ($("monthSelect")) $("monthSelect").value = new Date().getMonth() + 1;
    if ($("yearSelect")) $("yearSelect").innerHTML = `<option value="${new Date().getFullYear()}" selected>${new Date().getFullYear()}</option>`;
    updateBanner();
}
/**
 * Updates the alert banner text based on the selected view mode
 */
function updateBanner() {
    if (!$("alertBannerText"))
    {

        return;
    }
    $("alertBannerText").innerHTML = $("viewModeSelect")?.value === "PendingRequests"
        ? `Showing pending leave requests`
        : `Showing history leave requests for <span class="fw-bold">${$("monthSelect")?.options[$("monthSelect").selectedIndex].text} ${$("yearSelect")?.value}</span>`;
}
/**
 * Fetches leave request data from the server and populates the table
 */
function loadLeaveData() {
    const mode = $("viewModeSelect").value;
    let url = `/Leave/GetLeaveRequests?mode=${encodeURIComponent(mode)}&page=${currentPage}&pageSize=${pageSize}`;
    if (mode === "LeaveHistory" && $("monthSelect") && $("yearSelect")) url += `&month=${$("monthSelect").value}&year=${$("yearSelect").value}`;

    fetch(url).then(res => res.json()).then(data => {
        if (!$("leaveTableBody"))
        {
            return;
        }
            
        $("leaveTableBody").innerHTML = "";

        if (!data.items || !data.items.length)
        {
            const msg = (mode === "LeaveHistory" && $("monthSelect")) ? `There is no data for ${$("monthSelect").options[$("monthSelect").selectedIndex].text}.` : "No records found.";
            $("leaveTableBody").innerHTML = `<tr><td colspan='8' class='text-center text-secondary py-4 fw-medium'>${msg}</td></tr>`;
            if ($("paginationInfo")) $("paginationInfo").innerText = "Showing 0 to 0 of 0 records";
            if ($("paginationContainer")) $("paginationContainer").innerHTML = "";
            return;
        }

        const pills = { Pending: "orange-pill", Approved: "green-pill", Rejected: "red-pill" };
        const esc = str => str ? str.replace(/'/g, "\\'") : "";

        data.items.forEach(item => {
            const empId = `EMP${item.user_Id.toString().padStart(3, '0')}`;
            $("leaveTableBody").innerHTML += `
                <tr class="border-bottom border-light-subtle text-nowrap">
                    <td class="px-2 py-3">${empId}</td>
                    <td class="px-2 py-3">${item.user_Name}</td>
                    <td class="px-2 py-3">${item.department}</td>
                    <td class="px-2 py-3">${item.leaveType}</td>
                    <td class="px-2 py-3">${item.dateRange}</td>
                    <td class="px-2 py-3">${item.total_days}</td>
                    <td class="px-2 py-3"><span class="border rounded-pill d-inline-block text-center ${pills[item.status] || 'gray-pill'}">${item.status}</span></td>
                    <td class="px-3 py-3 text-center">
                        <a href="javascript:void(0)" class="text-primary fw-medium text-decoration-none" 
                           onclick="handleAdminViewClick('${esc(item.leaveType)}', '${item.startDate}', '${item.endDate}', '${item.total_days}', '${esc(item.reason)}', '${esc(item.user_Name)}', '${empId}', '${esc(item.attachment)}', '${item.leaveRequestId}')">View Details</a>
                    </td>
                </tr>`;
        });
        if (typeof renderPagination === "function") renderPagination(data.totalRecords, data.items.length, data.currentPage, data.totalPages);
    }).catch(() => {
        const tbody = $("leaveTableBody");
        if (tbody)
        {
            tbody.innerHTML = `<tr><td colspan='8' class='text-center text-danger'>Unable to load data. Please try again later.</td></tr>`;
        }
    });
}

/**
 * Renders pagination controls
 * @param {number} totalRecords 
 * @param {number} currentItemsCount 
 * @param {number} page 
 * @param {number} totalPages 
 */
function renderPagination(totalRecords, currentItemsCount, page, totalPages) {
    if (!$("paginationInfo") || !$("paginationContainer"))
    { 
        return;
    }
      
    $("paginationInfo").innerText = `Showing ${totalRecords === 0 ? 0 : (page - 1) * pageSize + 1} to ${(page - 1) * pageSize + currentItemsCount} of ${totalRecords} records`;
    $("paginationContainer").innerHTML = "";
    if (totalPages <= 1)
    {
        return;
    }
        

    const addBtn = (lbl, target, disabled, active) => {
        $("paginationContainer").innerHTML += `<button class="${active ? 'btn btn-primary active' : 'btn btn-outline-secondary'} ${disabled ? 'disabled' : ''}" ${disabled || active ? '' : `onclick="changePage(${target})"`}>${lbl}</button>`;
    };
    addBtn("Previous", page - 1, page <= 1, false);
    for (let i = 1; i <= totalPages; i++) addBtn(i, i, false, i === page);
    addBtn("Next", page + 1, page >= totalPages, false);
}

const changePage = (p) => { currentPage = p; loadLeaveData(); };
/**
 * Formats date string to YYYY-MM-DD for input fields
 */
const formatToInputDate = (str) => str ? new Date(str.includes("T") ? str.split("T")[0] : str).toISOString().split('T')[0] : "";
/**
 * Opens the leave modal and populates fields based on the selected mode
 */
function openLeaveModal(mode, leaveType, start, end, total, reason, name, id, file, reqId) {
    $("modalLeaveRequestId").value = reqId || '';
    $("modalLeaveType").value = leaveType || 'Sick Leave';
    $("modalStartDate").value = formatToInputDate(start);
    $("modalEndDate").value = formatToInputDate(end);
    $("modalTotalDays").value = total || '0';
    $("modalReason").value = reason || '';
    if (name) $("modalEmpName").value = name;
    if (id) $("modalEmpId").value = id;

    const isView = mode === 'view', isDecision = mode === 'decision';

    $("modalTitle").innerText = isView ? "View Leave Request Details" : isDecision ? "Leave Decision Form" : mode === 'update' ? "Update Leave" : "Request a Leave";
    $("modalSubTitle").innerText = isView ? "Review submitted leave form statements logs." : isDecision ? "Please review the leave request details and provide your decision." : mode === 'update' ? "Modify your pending request parameters below." : "Please fill in the details below to request leave.";
    $("modalSubmitBtn").innerText = mode === 'update' ? "Save Changes" : "Apply";

    $("modalSubmitBtn").classList.toggle('d-none', isView || isDecision);
    $("attachmentSection").classList.toggle('d-none', isView || isDecision);
    $("remarkSection").classList.toggle('d-none', !isDecision);
    $("btnAccept").classList.toggle('d-none', !isDecision);
    $("btnReject").classList.toggle('d-none', !isDecision);

    if (file && (isView || isDecision))
    {
        $("adminAttachmentLinkSection").classList.remove('d-none');
        $("modalAttachmentLink").innerText = file;
        $("modalAttachmentLink").href = "/uploads/leaves/" + file;
    } else $("adminAttachmentLinkSection").classList.add('d-none');

    if (isDecision && $("modalRemark"))
    {
        $("modalRemark").removeAttribute('disabled'); $("modalRemark").value = '';
    }

    [$("modalLeaveType"), $("modalStartDate"), $("modalEndDate"), $("modalReason")].forEach(f => {
        if (!f) return;
        f.toggleAttribute('disabled', isView || isDecision);
        f.classList.toggle('custom-bg-light', isView || isDecision);
        f.classList.toggle('custom-not-allowed', isView || isDecision);
    });

    document.querySelectorAll('.dynamic-star').forEach(s => s.classList.toggle('d-none', isView || isDecision));
    if ($("popupToggle")) $("popupToggle").checked = true;
}
/**
 * Handles the leave request status change (Approval/Rejection)
 */
function handleLeaveDecision(decision) {
    fetch('/Leave/UpdateLeaveStatus', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ LeaveRequestId: $("modalLeaveRequestId").value, Status: decision, Remark: $("modalRemark").value })
    }).then(res => res.json()).then(data => {
        if (data.success)
        {
            alert("Decision saved successfully!"); location.reload();
        }
        else alert("Error: " + data.message);
    });
}

/**
 * Opens the leave detail modal in 'decision' or 'view' mode based on the current context.
 * 
 * @param {string} leaveType - Type of leave.
 * @param {string} start - Start date.
 * @param {string} end - End date.
 * @param {string|number} total - Total days requested.
 * @param {string} reason - Leave reason.
 * @param {string} name - Employee name.
 * @param {string} id - Employee ID.
 * @param {string} file - Attachment filename.
 * @param {string|number} reqId - Unique leave request ID.
 */
function handleAdminViewClick(leaveType, start, end, total, reason, name, id, file, reqId) {
    openLeaveModal($("viewModeSelect").value === "PendingRequests" ? 'decision' : 'view', leaveType, start, end, total, reason, name, id, file, reqId);
}