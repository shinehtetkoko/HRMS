//company profile

function switchToEditMode() {
    document.getElementById("viewMode").classList.add("d-none");
    document.getElementById("editMode").classList.remove("d-none");

    document.getElementById("editBtn").classList.add("d-none");

    document.getElementById("pageTitle").innerText =
        "Edit Company Profile";
}

function switchToViewMode() {
    document.getElementById("viewMode").classList.remove("d-none");
    document.getElementById("editMode").classList.add("d-none");

    document.getElementById("editBtn").classList.remove("d-none");

    document.getElementById("pageTitle").innerText =
        "Company Profile";
}

async function saveProfile(event) {
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

            switchToViewMode();

            // success
            const toastEl = document.getElementById('successToast');
            if (toastEl) { new bootstrap.Toast(toastEl).show(); }

            setTimeout(() => { window.location.reload(); }, 1500);

        } else {

            // error
            const errResult = await response.json().catch(() => ({}));
            console.log("Server Validation Error Details:", errResult);

            const errorToastEl = document.getElementById('errorToast');
            if (errorToastEl) {
                new bootstrap.Toast(errorToastEl).show();
            }
        }
    } catch (error) {
        console.error('Error:', error);
        const errorToastEl = document.getElementById('errorToast');
        if (errorToastEl) { new bootstrap.Toast(errorToastEl).show(); }
    }
}


// HR Account Registeration Form

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
            const selectedGender = genderRadio ? genderRadio.value : "1"; // Not selected Male(1)

            const roleIdElement = document.getElementById("regRoleId");
            const currentRoleId = roleIdElement ? parseInt(roleIdElement.value) : 2;

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

            const submitUrl = currentRoleId === 3
                ? '/Admin/RegisterEmployeeAccount'
                : '/Admin/RegisterHRAccount';

            const redirectUrl = currentRoleId === 3
                ? '/Admin/EmployeeDirectory'
                : '/Admin/HRDirectory';

            try {
                // Sent data to Controller by Fetch API dynamically
                const response = await fetch(submitUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(requestData)
                });

                const result = await response.json();

                if (response.ok && result.success) {
                    alert(result.message); // Success Alert

                    const toggleCheckbox = document.getElementById('registerToggle');
                    if (toggleCheckbox) toggleCheckbox.checked = false;

                    window.location.href = redirectUrl;
                } else {
                    alert(result.message || "Account registration failed.");
                    submitBtn.disabled = false;
                    submitBtn.innerText = originalBtnText;
                }
            } catch (error) {
                console.error("Error:", error);
                alert("Network error or server timeout. Please try again later.");
                submitBtn.disabled = false;
                submitBtn.innerText = originalBtnText;
            }
        });
    }
});

// Edit/View HR Account

async function openEditModal(userId, isHRDirectory = false, isMyTeam = false) {

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

            const cssToggle = document.getElementById("editToggle-EMP001");
            if (cssToggle) {
                cssToggle.checked = true;
            }
        } else {
            alert("Failed to load employee profile data.");
        }
    } catch (error) {
        console.error("Error loading edit modal:", error);
        alert("An error occurred while opening the profile.");
    }
}

function closeEditModal() {
    const toggle = document.getElementById("editToggle-EMP001");
    if (toggle) {
        toggle.checked = false;
    }

    setTimeout(() => {
        const modalContainer = document.getElementById("editModalContainer");
        if (modalContainer) modalContainer.innerHTML = "";
    }, 200);
}

function toggleResignFields() {
    var status = document.getElementById("empStatusEdit").value;
    var panel = document.getElementById("resignFields");
    if (status === "Resigned") {
        panel.classList.remove("d-none");
    } else {
        panel.classList.add("d-none");
    }
}

async function submitEmployeeUpdate(event) {
    event.preventDefault();

    const payload = {
        User_Id: parseInt(document.getElementById("hdnUserId").value),
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
            closeEditModal();
            window.location.reload();
        } else {
            alert("Error: " + (result.message || "Failed to update profile."));
        }
    } catch (error) {
        console.error("Submission error:", error);
        alert("An error occurred while saving changes.");
    }
}




