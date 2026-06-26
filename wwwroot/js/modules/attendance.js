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
/**
 * Redirects the page to view attendance logs filtered by the selected month.
 */
function filterAttendance() {
    const monthSelect = document.getElementById("monthSelect");
    if (monthSelect) {
        window.location.href = `/Attendance/AttendanceHistory?month=${monthSelect.value}`;
    }
}

/**
 * Resets all active filters and reloads the base attendance history page.
 */
function resetFilter() {
    window.location.href = '/Attendance/AttendanceHistory';
}

/**
 * Setup and updates the UI components for month filters during page initialization.
 */
document.addEventListener("DOMContentLoaded", function () {
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
//--------End Attendance History Filters------------

