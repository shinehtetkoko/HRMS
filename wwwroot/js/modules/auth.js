// Employee Login Logic
function processLogin() {
    // Read HTML Input fields 
    const emailVal = document.getElementById("email").value.trim();
    const passwordVal = document.getElementById("password").value.trim();

    const formData = new FormData();
    formData.append("Input.Email", emailVal);
    formData.append("Input.Password", passwordVal);

    // Backend Controller 
    fetch('/Auth/Login', {
        method: 'POST',
        body: formData
    })
        .then(response => {

            if (response.redirected) {
                window.location.href = response.url;
                return;
            }
            return response.text();
        })
        .then(html => {
            if (html) {

                document.open();
                document.write(html);
                document.close();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert("Server connection error!");
        });
}


document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", function (e) {
            e.preventDefault();
            processLogin();
        });
    }
});


//change/ reset password
function togglePasswordVisibility(inputId, button) {

    const input = document.getElementById(inputId);
    const icon = button.querySelector("i");

    if (input.type === "password") {
        input.type = "text";
        icon.className = "bi bi-eye";
    } else {
        input.type = "password";
        icon.className = "bi bi-eye-slash";
    }
}

const passwordForm = document.getElementById('passwordForm');
if (passwordForm) {
    passwordForm.addEventListener('submit', function (event) {

        const currentPwdField =
            document.getElementById('current-password');

        const newPwd =
            document.getElementById('new-password').value;

        const confirmPwd =
            document.getElementById('confirm-password').value;

        const passwordError =
            document.getElementById('password-error');

        const errorText =
            document.getElementById('match-error');

        const isCurrentPasswordHidden = currentPwdField ? currentPwdField.closest('.mb-3').classList.contains('d-none') : true;

        if (!isCurrentPasswordHidden) {
            // First Time Login
            if (!currentPwdField.value) {
                alert("Please fill in your Current Password.");
                event.preventDefault();
                return;
            }
        }

        if (!newPwd || !confirmPwd) {
            alert("Fill in all required password fields.");
            event.preventDefault();
            return;
        }

        // Check Password Complexity
        const regex =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@@$!%*?&])[A-Za-z\d@@$!%*?&]{8,50}$/;

        if (!regex.test(newPwd)) {

            event.preventDefault();

            passwordError.classList.remove('d-none');

            return;
        }
        else {
            passwordError.classList.add('d-none');
        }


        if (newPwd !== confirmPwd) {
            event.preventDefault();
            errorText.classList.remove('d-none');
            return;
        }

        errorText.classList.add('d-none');

        const toastEl = document.getElementById('success-toast');
        if (toastEl) {
            const toast = new bootstrap.Toast(toastEl);
            toast.show();
        }
    });
}

// Forgot Password Request Logic
async function handleResetRequest(event) {
    event.preventDefault();

    const form = event.target;
    const submitBtn = document.getElementById('submitBtn');
    const successBanner = document.getElementById('successBanner');

    // Loading State
    submitBtn.disabled = true;
    submitBtn.innerText = "Sending Link...";
    submitBtn.classList.add('disabled-btn');

    try {
        const response = await fetch('/Auth/ForgotPassword', {
            method: 'POST',
            body: new FormData(form)
        });

        const contentType = response.headers.get("content-type");
        let data = {};
        if (contentType && contentType.indexOf("application/json") !== -1) {
            data = await response.json();
        }

        if (response.ok) {
            if (successBanner) {
                successBanner.classList.remove('d-none');
            }
            form.reset();

            setTimeout(() => {
                resetButtonState(submitBtn);
            }, 3000);

        } else {
            alert(data.message || "This email address is not registered in our system.");
            resetButtonState(submitBtn);
        }

    } catch (error) {
        // Handle network/connection failures
        console.error('Error:', error);
        alert("Network error. Please check your connection.");
        resetButtonState(submitBtn);
    }
}

function resetButtonState(submitBtn) {
    if (submitBtn) {
        submitBtn.disabled = false;
        submitBtn.innerText = "Send Reset Link";
        submitBtn.classList.remove('disabled-btn');
    }
}