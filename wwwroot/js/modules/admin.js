//--------------------Start Company Profile--------------------//
/**
 * Switches the company profile layout into editable mode.
 */
function switchToCompanyProfileEditMode() {
    document.getElementById("viewMode").classList.add("d-none");
    document.getElementById("editMode").classList.remove("d-none");
    document.getElementById("editBtn").classList.add("d-none");
    document.getElementById("pageTitle").innerText = "Edit Company Profile";       
}

/**
 * Switches the company profile layout back into read-only display mode.
 */
function switchToCompanyProfileViewMode() {
    document.getElementById("viewMode").classList.remove("d-none");
    document.getElementById("editMode").classList.add("d-none");
    document.getElementById("editBtn").classList.remove("d-none");
    document.getElementById("pageTitle").innerText = "Company Profile";       
}

/**
 * Collects input data and sends a request to update the company profile details.
 * @param {Event} event - The native form submission event object.
 */
async function saveCompanyProfile(event) {
    event.preventDefault();

    const model = {
        Comp_Id: parseInt(document.getElementById('companyId').value) || 0,
        Comp_Name: document.getElementById('companyName').value,
        Comp_Ph_No: document.getElementById('phone').value,
        Comp_Email: document.getElementById('email').value,
        Comp_Location: document.getElementById('location').value,
        Description: document.getElementById('description').value,
    };

    try {
        const response = await fetch('/Admin/UpdateCompanyProfile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(model)
        });

        if (response.ok) {

            document.getElementById("viewPhone").innerText = model.Comp_Ph_No;
            document.getElementById("viewEmail").innerText = model.Comp_Email;
            document.getElementById("viewLocation").innerText = model.Comp_Location;
            document.getElementById("viewDescription").innerText = model.Description;

            switchToCompanyProfileViewMode();

            const toastEl = document.getElementById('successToast');
            if (toastEl) { new bootstrap.Toast(toastEl).show(); }

            setTimeout(() => { window.location.reload(); }, 1500);

        } else {  
            const errorToastEl = document.getElementById('errorToast');
            if (errorToastEl) {
                new bootstrap.Toast(errorToastEl).show();
            }
        }
    } catch (error) {       
        const errorToastEl = document.getElementById('errorToast');
        if (errorToastEl) { new bootstrap.Toast(errorToastEl).show(); }
    }
}
//----------End Company Profile------------

//-------Start HR Account Registeration----
document.addEventListener("DOMContentLoaded", function () {
    const hrForm = document.getElementById("hrRegisterForm");

    if (hrForm) {
        hrForm.addEventListener("submit", async function (event) {
            event.preventDefault();

            const submitBtn = document.getElementById("submitModalBtn");
            const originalBtnText = submitBtn.innerText;

            submitBtn.disabled = true;
            submitBtn.innerText = "Registering & Sending Mail...";

            const genderRadio = document.querySelector('input[name="regGender"]:checked');
            const selectedGender = genderRadio ? genderRadio.value : "1"; 

            const roleIdElement = document.getElementById("regRoleId");
            const currentRoleId = roleIdElement ? parseInt(roleIdElement.value) : 2;

            const wrapper = document.querySelector("[data-employee-role]");
            const EMPLOYEE_ROLE_ID = wrapper ? parseInt(wrapper.dataset.employeeRole) : 3; 

            const isEmployee = currentRoleId === EMPLOYEE_ROLE_ID;

            const requestData = {
                User_Name: document.getElementById("regName").value,
                Dept_Id: parseInt(document.getElementById("regDeptId").value),
                Gender: parseInt(selectedGender),
                Nrc: document.getElementById("regNrc").value,
                Dob: document.getElementById("regDob").value,
                Married_Status: parseInt(document.getElementById("regMarriedStatus").value),
                Position: document.getElementById("regPosition").value,
                Hired_Date: document.getElementById("regHiredDate").value,
                Qualification: document.getElementById("regQualification").value,
                User_Ph_No: document.getElementById("regPhone").value,
                Address: document.getElementById("regAddress").value,
                Email: document.getElementById("regEmail").value,
                Role_Id: currentRoleId
            };

            const submitUrl = currentRoleId === isEmployee 
                ? '/Admin/RegisterEmployeeAccount'
                : '/Admin/RegisterHRAccount';

            const redirectUrl = currentRoleId === isEmployee
                ? '/Admin/EmployeeDirectory'
                : '/Admin/HRDirectory';

            try {
                const response = await fetch(submitUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(requestData)
                });

                const result = await response.json();

                if (response.ok && result.success) {
                    alert(result.message); 
                    const toggleCheckbox = document.getElementById('registerToggle');
                    if (toggleCheckbox) toggleCheckbox.checked = false;

                    window.location.href = redirectUrl;
                } else {
                    alert(result.message || "Account registration failed.");
                    submitBtn.disabled = false;
                    submitBtn.innerText = originalBtnText;
                }
            } catch (error) {             
                alert("Network error or server timeout. Please try again later.");
                submitBtn.disabled = false;
                submitBtn.innerText = originalBtnText;
            }
        });
    }
});

/**
 * Loads profile edit content from the server and injects it into a popup modal container.
 * @param {any} userId - The unique identifier of the user to edit.
 * @param {any} isHRDirectory - Flag checking if the request came from the HR Directory screen.
 * @param {any} isMyTeam - Flag checking if the request came from the Team layout page.
 */
async function openHRDirectoryEditModal(userId, isHRDirectory = false, isMyTeam = false) {

    let modalContainer = document.getElementById("editModalContainer");
    if (!modalContainer) {
        modalContainer = document.createElement("div");
        modalContainer.id = "editModalContainer";
        document.body.appendChild(modalContainer);
    }

    try {
        const response = await fetch(`/Admin/GetEditProfilePopup?userId=${userId}&isHRDirectory=${isHRDirectory}&isMyTeam=${isMyTeam}`);

        if (response.ok) {
            const htmlContent = await response.text();
            modalContainer.innerHTML = htmlContent;

            const cssToggle = document.getElementById(`editToggle-${userId}`);
            if (cssToggle) {
                cssToggle.checked = true;
            }
        } else {
            alert("Failed to load employee profile data.");
        }
    } catch (error) {      
        alert("An error occurred while opening the profile.");
    }
}

/**
 * Closes the active profile edit popup panel and resets container content.
 */
function closeHRDirectoryEditModal() {
    const toggle = document.getElementById(`editToggle-${userId}`);
    if (toggle) {
        toggle.checked = false;
    }

    setTimeout(() => {
        const modalContainer = document.getElementById("editModalContainer");
        if (modalContainer) modalContainer.innerHTML = "";
    }, 200);
}

/**
 * Toggles the visibility of resignation form fields based on the selected account status value.
 */
function toggleEmployeeResignStatusFields() {
    var status = document.getElementById("empStatusEdit").value;
    var panel = document.getElementById("resignFields");
    if (status === "Resigned") {
        panel.classList.remove("d-none");
    } else {
        panel.classList.add("d-none");
    }
}

/**
 * Submits updated account status data and resignation records to the server.
 * @param {Event} event - The native form submission event object.
 */
async function submitEmployeeUpdate(event) {
    event.preventDefault();

    const userIdVal = parseInt(document.getElementById("hdnUserId").value);
    const payload = {
        User_Id: userIdVal,
        AccountStatus: document.getElementById("empStatusEdit") ? document.getElementById("empStatusEdit").value : "Active",
        ResignDateStr: document.getElementById("txtResignDate") ? document.getElementById("txtResignDate").value : null,
        ResignReason: document.getElementById("txtResignReason") ? document.getElementById("txtResignReason").value : null
    };

    try {
        const response = await fetch('/Admin/UpdateHRStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        const result = await response.json();

        if (response.ok && result.success) {
            alert(result.message);
            closeHRDirectoryEditModal(userIdVal);
            window.location.reload();
        } else {
            alert("Error: " + (result.message || "Failed to update profile."));
        }
    } catch (error) {
        alert("An error occurred while saving changes.");
    }
}




