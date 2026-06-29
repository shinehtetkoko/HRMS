/**
 * validateProfileForm
 * @returns {boolean}
 */
function validateProfileForm() {
    var phone = document.getElementsByName("NewPhoneNumber")[0].value.trim();
    var address = document.getElementsByName("NewAddress")[0].value.trim();

    if (phone === "" && address === "") {
        alert("Please fill out at least one field (Phone or Address) to submit.");
        return false; 
    }
    return true; 
}

/**
 * Controls the profile notification red dot visibility
 */
document.addEventListener("DOMContentLoaded", function () {
    const currentUrl = window.location.href.toLowerCase();

    if (currentUrl.includes("/employee/profile")) {
        document.cookie = "hasProfileNoti=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC; SameSite=Lax;";
    }

    else if (document.cookie.includes("hasProfileNoti=true")) {
        const dot = document.getElementById('profileNotiDot');
        if (dot) {
            dot.classList.remove('d-none');
        }
    }
});

/**
 * Employee Filter, Reset, Export Management
 */
document.addEventListener("DOMContentLoaded", function () {
    const $ = id => document.getElementById(id);  
    const tableBody = $("employeeTableBody");
    if (!tableBody) return;

    function loadEmployeeData() {
        const status = statusSelect ? statusSelect.value : "Active";
        const dept = deptSelect ? deptSelect.value : "All";

        fetch(`/Employee/GetFilteredEmployees?status=${status}&department=${dept}`)
            .then(response => response.text()) 
            .then(html => {
                tableBody.innerHTML = html;
            })
            .catch(error => console.error("Error loading data:", error));
    }

    document.querySelector(".filter-btn")?.addEventListener("click", loadEmployeeData);

    document.querySelector(".reset-btn")?.addEventListener("click", () => {
        if ($("statusSelect")) $("statusSelect").value = "Active";
        if ($("deptSelect")) $("deptSelect").value = "All";
        loadEmployeeData();
    });

    $("exportExcelBtn")?.addEventListener("click", () => {
        const status = $("statusSelect")?.value || "Active";
        const dept = $("deptSelect")?.value || "All";
        window.location.href = `/Employee/ExportEmployeeToExcel?status=${status}&department=${dept}`;
    });
});

/**
 * handleExcelImport
 * @param {any} event
 * @returns
 */
async function handleExcelImport(event) {
    const file = event.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    const token = document.getElementById("RequestVerificationToken").value;

    const response = await fetch('/Employee/Import', {
        method: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        body: formData
    });

    if (response.headers.get("content-type").includes("application/vnd.openxmlformats")) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = "ErrorReport.xlsx";
        a.click();
        alert("Import finished with errors. Error report has been downloaded.");
    } else {
        const result = await response.json();
        alert(result.message);
        if (result.success) location.reload();
    }
    
}
