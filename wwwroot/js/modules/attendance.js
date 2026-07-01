//-------Start Attendance Check-In / Out------------
/**
 * Toggles the visibility of attachment and location fields based on office or remote selection.
 */
function toggleLocation() {

    const workLocationRadio = document.querySelector('input[name="workLocation"]:checked');
    if (!workLocationRadio) return;

    const selected = workLocationRadio.value;
    const checkInMode = document.getElementById("checkInMode");
    const attachment = document.getElementById("attachmentSection");
    const location = document.getElementById("locationSection");

    if (selected === "office") {
        if (checkInMode) checkInMode.value = "Standard";
        if (attachment) attachment.classList.add("d-none");
        if (location) location.classList.add("d-none");
    } else {
        if (checkInMode) checkInMode.value = "Remote";
        if (attachment) attachment.classList.remove("d-none");
        if (location) location.classList.remove("d-none");
    }
}

/**
 * Validates check-in inputs and submits the multipart FormData to the server.
 */
function validateCheckIn() {
    const workLocationRadio = document.querySelector('input[name="workLocation"]:checked');
    if (!workLocationRadio) return;
    const selected = workLocationRadio.value;

    const attachmentError = document.getElementById("attachment-error");
    const fileTypeError = document.getElementById("filetype-error");
    const locationError = document.getElementById("location-error");

    if (attachmentError) attachmentError.classList.add("d-none");
    if (fileTypeError) fileTypeError.classList.add("d-none");
    if (locationError) locationError.classList.add("d-none");

    if (selected !== "office") {
        const file = document.getElementById("fileInput");
        const locationDetails = document.getElementById("locationDetails");
        const locationText = locationDetails ? locationDetails.value.trim() : "";

        if (locationText === "") {
            if (locationError) locationError.classList.remove("d-none");
            return;
        }

        if (!file || file.files.length === 0) {
            if (attachmentError) attachmentError.classList.remove("d-none");
            return;
        }

        const fileName = file.files[0].name.toLowerCase();
        const valid = fileName.endsWith(".jpg") || fileName.endsWith(".jpeg") || fileName.endsWith(".png");

        if (!valid) {
            if (fileTypeError) fileTypeError.classList.remove("d-none");
            return;
        }
    }

    const formData = new FormData();
    formData.append("WorkLocation", selected);

    const checkInModeObj = document.getElementById("checkInMode");
    formData.append("CheckInMode", checkInModeObj ? checkInModeObj.value : "Standard");

    const locationDetailsObj = document.getElementById("locationDetails");
    formData.append("LocationDetails", locationDetailsObj ? locationDetailsObj.value.trim() : "");

    const fileObj = document.getElementById("fileInput");
    if (fileObj && fileObj.files.length > 0) {
        formData.append("Attachment", fileObj.files[0]);
    }

    fetch('/Attendance/DailyCheckIn', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);

                const checkInBtn = document.getElementById("checkInBtn");
                const checkOutBtn = document.getElementById("checkOutBtn");

                if (checkInBtn && checkOutBtn) {
                    checkInBtn.disabled = true;
                    checkInBtn.classList.add("opacity-50");
                    checkOutBtn.disabled = false;
                    checkOutBtn.classList.remove("opacity-50");
                    checkOutBtn.style.cursor = "pointer";
                }
                window.location.href = '/Attendance/AttendanceHistory';
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            alert("Unexpected error when connecting with server!");
        });
}

/**
 * Processes the daily check-out request after user confirmation.
 */
function validateCheckOut() {
    if (!confirm("Are you sure you want to check out for today?")) {
        return;
    }

    fetch('/Attendance/DailyCheckOut', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
                window.location.href = '/Attendance/AttendanceHistory';
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            alert("Unexpected error during check-out connection!");
        });
}

/**
 * Initializes the page events and setups layout triggers.
 */
document.addEventListener("DOMContentLoaded", function () {
    toggleLocation();

    const checkForm = document.getElementById("checkForm");
    if (checkForm) {
        checkForm.addEventListener("submit", function (e) {
            e.preventDefault();
            validateCheckIn();
        });
    }
});
//---------End Attendance Check-In / Out------------


//-------Start Attendance History Filters-----------

let currentPage = 1;
const recordsPerPage = 10;

/**
 * Initializes the pagination system and syncs the month dropdown setup 
 * with the current URL parameter or Razor model values on page load.
 * 
 * @requires HTML_Elements: #monthSelect, #infoText, #yearDisplay
 * @calls initPaginationSystem
 * @returns {void}
 */
document.addEventListener("DOMContentLoaded", function () {
    initPaginationSystem();

    const monthSelect = document.getElementById("monthSelect");
    const infoText = document.getElementById("infoText");
    const yearDisplay = document.getElementById("yearDisplay");

    if (monthSelect && infoText) {
        const razorSelectedMonth = monthSelect.getAttribute("data-selected");
        const urlParams = new URLSearchParams(window.location.search);
        const monthParam = urlParams.get('month');

        const currentDate = new Date();
        const currentYear = yearDisplay ? yearDisplay.innerText.trim() : currentDate.getFullYear();

        if (monthParam) {
            monthSelect.value = monthParam;
        } else if (razorSelectedMonth) {
            monthSelect.value = razorSelectedMonth;
        } else {
            monthSelect.value = currentDate.getMonth() + 1;
        }

        const currentOption = monthSelect.querySelector(`option[value="${monthSelect.value}"]`);
        if (currentOption) {
            const selectedMonthText = currentOption.text;
            infoText.innerText = `${selectedMonthText} ${currentYear}`;
        }
    }
});

/**
 * Calculates total records and pages from the DOM table rows, updates statistical indicators, 
 * and dynamic builds the DOM structure for prev, next, and individual page items.
 * 
 * @requires HTML_Elements: .custom-table-tbody, #lblTotalPages, #lblCurrentPage, #lblTotalRecords, #btnPrevItem, #dynamicPageButtons, #btnNextItem
 * @calls renderTableRowsOnly
 * @returns {void}
 */
function initPaginationSystem() {
    const tableBody = document.querySelector(".custom-table-tbody");
    if (!tableBody) return;

    const rows = tableBody.querySelectorAll("tr");

    const hasRealData = rows.length > 0 && !rows[0].querySelector('td[colspan]');
    const totalRecords = hasRealData ? rows.length : 0;
    const totalPages = Math.ceil(totalRecords / recordsPerPage) || 1;

    if (document.getElementById('lblTotalPages')) document.getElementById('lblTotalPages').innerText = totalPages;
    if (document.getElementById('lblCurrentPage')) document.getElementById('lblCurrentPage').innerText = currentPage;
    if (document.getElementById('lblTotalRecords')) document.getElementById('lblTotalRecords').innerText = totalRecords;

    const prevItem = document.getElementById('btnPrevItem');
    if (prevItem) {
        prevItem.innerHTML = '';
        const prevBtn = document.createElement('button');
        prevBtn.className = "btn btn-sm btn-outline-secondary text-dark px-3 py-1";
        prevBtn.style.borderRadius = "6px";
        prevBtn.innerText = "Previous";
        prevBtn.onclick = () => handlePreviousAndNext('prev', totalPages);
        prevItem.appendChild(prevBtn);
    }

    const container = document.getElementById('dynamicPageButtons');
    if (container) {
        container.innerHTML = '';
        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement('button');
            btn.className = `btn btn-sm px-3 py-1 fw-medium ${i === currentPage ? 'btn-primary' : 'btn-outline-secondary text-dark'}`;
            btn.style.borderRadius = '6px';
            btn.innerText = i;
            btn.onclick = () => {
                currentPage = i;
                renderTableRowsOnly();
            };

            const li = document.createElement('li');
            li.className = `page-item ${i === currentPage ? 'active' : ''}`;
            li.appendChild(btn);
            container.appendChild(li);
        }
    }

    const nextItem = document.getElementById('btnNextItem');
    if (nextItem) {
        nextItem.innerHTML = '';
        const nextBtn = document.createElement('button');
        nextBtn.className = "btn btn-sm btn-outline-secondary text-dark px-3 py-1";
        nextBtn.style.borderRadius = "6px";
        nextBtn.innerText = "Next";
        nextBtn.onclick = () => handlePreviousAndNext('next', totalPages);
        nextItem.appendChild(nextBtn);
    }

    renderTableRowsOnly();
}

/**
 * Filters and toggles the CSS display property of individual table body rows 
 * based on the selected page segment index, and highlights active button configurations.
 * 
 * @requires HTML_Elements: .custom-table-tbody, #lblCurrentPage, #dynamicPageButtons
 * @calls updateButtonStates
 * @returns {void}
 */
function renderTableRowsOnly() {
    const tableBody = document.querySelector(".custom-table-tbody");
    if (!tableBody) return;

    const rows = tableBody.querySelectorAll("tr");
    const totalRecords = rows.length;
    const totalPages = Math.ceil(totalRecords / recordsPerPage) || 1;

    rows.forEach((row, index) => {
        const pageIndex = Math.floor(index / recordsPerPage) + 1;
        if (pageIndex === currentPage) {
            row.style.setProperty('display', 'table-row', 'important');
        } else {
            row.style.setProperty('display', 'none', 'important');
        }
    });

    if (document.getElementById('lblCurrentPage')) document.getElementById('lblCurrentPage').innerText = currentPage;

    const container = document.getElementById('dynamicPageButtons');
    if (container) {
        const listItems = container.querySelectorAll('li');
        listItems.forEach((li, index) => {
            const pageNum = index + 1;
            const btn = li.querySelector('button');
            if (pageNum === currentPage) {
                li.classList.add('active');
                if (btn) {
                    btn.className = "btn btn-sm px-3 py-1 fw-medium btn-primary";
                }
            } else {
                li.classList.remove('active');
                if (btn) {
                    btn.className = "btn btn-sm px-3 py-1 fw-medium btn-outline-secondary text-dark";
                }
            }
        });
    }

    updateButtonStates(totalPages);
}

/**
 * Handles index modifications for previous and next operations, 
 * updating the active page boundaries safely without overflowing boundaries.
 * 
 * @param {string} direction - Target execution command sequence ('prev' or 'next')
 * @param {number} totalPages - Total available pages calculated for evaluation
 * @calls renderTableRowsOnly
 * @returns {void}
 */
function handlePreviousAndNext(direction, totalPages) {
    if (direction === 'prev') {
        if (currentPage > 1) {
            currentPage--;
        }
    } else if (direction === 'next') {
        if (currentPage < totalPages) {
            currentPage++;
        }
    }
    renderTableRowsOnly();
}

/**
 * Manages the layout configuration and interaction accessibility limits for 
 * control buttons, introducing inline styles to denote restricted or clickable options.
 * 
 * @param {number} totalPages - Complete page count threshold to assess final targets
 * @requires HTML_Elements: #btnPrevItem, #btnNextItem
 * @returns {void}
 */
function updateButtonStates(totalPages) {
    const prevItem = document.getElementById('btnPrevItem');
    const nextItem = document.getElementById('btnNextItem');
    const prevBtn = prevItem?.querySelector('button');
    const nextBtn = nextItem?.querySelector('button');

    if (prevItem && prevBtn) {
        if (currentPage === 1) {
            prevItem.classList.add("disabled");
            prevBtn.style.opacity = '0.5';
            prevBtn.style.cursor = 'not-allowed';
        } else {
            prevItem.classList.remove("disabled");
            prevBtn.style.opacity = '1';
            prevBtn.style.cursor = 'pointer';
        }
    }

    if (nextItem && nextBtn) {
        if (currentPage === totalPages) {
            nextItem.classList.add("disabled");
            nextBtn.style.opacity = '0.5';
            nextBtn.style.cursor = 'not-allowed';
        } else {
            nextItem.classList.remove("disabled");
            nextBtn.style.opacity = '1';
            nextBtn.style.cursor = 'pointer';
        }
    }
}

/**
 * Captures selected values inside month fields to generate a fresh HTTP GET route, 
 * redirecting users to segmented attendance histories.
 * 
 * @requires HTML_Elements: #monthSelect
 * @returns {void}
 */
function filterAttendance() {
    const monthSelect = document.getElementById("monthSelect");
    if (monthSelect) {
        window.location.href = `/Attendance/AttendanceHistory?month=${monthSelect.value}`;
    }
}

/**
 * Clears parameters from modern search queries, reloading default states 
 * for active dashboard records.
 * 
 * @returns {void}
 */
function resetFilter() {
    window.location.href = '/Attendance/AttendanceHistory';
}
//--------End Attendance History Filters------------


//--------Start Attendance Record Filters------------
/**
 * Handles page index redirection for the attendance record tracking matrix,
 * verifying that the input index resides within valid boundary limits before firing queries.
 * 
 * @param {number} newPage - The targeted destination page sequence index to navigate toward
 * @requires HTML_Elements: #totalPages, #monthSelect, #yearDisplay, #deptSelect, #employeeSelect
 * @returns {void}
 */
function changePage(newPage) {
    const totalPagesInput = document.getElementById("totalPages");
    const totalPages = totalPagesInput ? parseInt(totalPagesInput.value) : 1;
    if (newPage < 1) {
        newPage = 1;
    }
    if (newPage > totalPages) {
        newPage = totalPages;
    }

    const month = document.getElementById("monthSelect")?.value || "";
    const year = document.getElementById("yearDisplay")?.innerText.trim() || "";
    const dept = document.getElementById("deptSelect")?.value || "";
    const employee = document.getElementById("employeeSelect")?.value || "";

    let url = `/Attendance/AttendanceRecord?month=${month}&year=${year}&page=${newPage}`;

    if (dept && dept !== "Select Department") {
        url += `&dept=${encodeURIComponent(dept)}`;
    }
    if (employee && employee !== "Select Employee") {
        url += `&employee=${encodeURIComponent(employee)}`;
    }

    window.location.href = url;
}

/**
 * Captures structural filters such as month, year, department, and employee selectors
 * to generate a refined dashboard lookup query sequence, reverting active pagination back to page 1.
 * 
 * @requires HTML_Elements: #monthSelect, #yearDisplay, #deptSelect, #employeeSelect
 * @returns {void}
 */
function filterRecords() {
    const month = document.getElementById("monthSelect")?.value || "";
    const year = document.getElementById("yearDisplay")?.innerText.trim() || "";
    const dept = document.getElementById("deptSelect")?.value || "";
    const employee = document.getElementById("employeeSelect")?.value || "";

    let url = `/Attendance/AttendanceRecord?month=${month}&year=${year}&page=1`;

    if (dept && dept !== "Select Department") {
        url += `&dept=${encodeURIComponent(dept)}`;
    }
    if (employee && employee !== "Select Employee") {
        url += `&employee=${encodeURIComponent(employee)}`;
    }

    window.location.href = url;
}

/**
 * Clears active dashboard search filters, redirecting the browser view
 * to the base Attendance Record entry point.
 * 
 * @returns {void}
 */
function resetRecordsFilter() {
    window.location.href = '/Attendance/AttendanceRecord';
}

/**
 * Attaches operational click event handlers to export targets, building out spreadsheet
 * download endpoints bundled with your active dashboard filter settings.
 * 
 * @requires HTML_Elements: #exportExcelBtn, #monthSelect, #yearDisplay, #deptSelect, #employeeSelect
 * @returns {void}
 */
document.addEventListener("DOMContentLoaded", function () {
    const exportBtn = document.getElementById("exportExcelBtn");
    if (exportBtn) {
        exportBtn.addEventListener("click", function () {
            const month = document.getElementById("monthSelect")?.value || "";
            const year = document.getElementById("yearDisplay")?.innerText.trim() || "";
            const dept = document.getElementById("deptSelect")?.value || "";
            const employee = document.getElementById("employeeSelect")?.value || "";

            window.location.href = `/Attendance/ExportToExcel?month=${month}&year=${year}&dept=${encodeURIComponent(dept)}&employee=${encodeURIComponent(employee)}`;
        });
    }
});
//--------End Attendance Record Filters------------
