//----------------------Start Leave Policy----------------------//
/**
 * Initializes the Leave Policy form behavior and validation.
 *
 * Features:
 * - Enables Carry Forward only for Annual Leave.
 * - Enables Max Carry Days only when Carry Forward is checked.
 * - Validates Total Available Days, Carry Forward, and Max Carry Days.
 * - Submits the form using AJAX after successful validation.
 * - Displays validation and server-side error messages.
 *
 * @returns {void}
 */
$(document).ready(function () {
    /**
    * Toggles Carry Forward and Max Carry Days controls
    * based on the selected leave type.
    *
    * Rules:
    * - Annual Leave → Enable Carry Forward option.
    * - Other Leave Types → Disable and reset Carry Forward settings.
    *
    * @returns {void}
    */
    function toggleCarryForward() {
        var selectedText = $("#LeaveType").find("option:selected").text().trim().toUpperCase();
        var isAnnual = (selectedText === "ANNUAL");
        if (isAnnual)
        {
            $("#carryForward").prop("disabled", false);
            $("#maxCarryDays").prop("disabled", !$("#carryForward").is(":checked"));
        }
        else
        {
            $("#carryForward").prop("disabled", true).prop("checked", false);
            $("#maxCarryDays").prop("disabled", true).val("");
        }
    }
    // Leave Type change event
    $("#LeaveType").change(function () {
        toggleCarryForward();
    });
    // Carry Forward checkbox change event
    $("#carryForward").change(function () {
        $("#maxCarryDays").prop("disabled", !$(this).is(":checked"));
        if (!$(this).is(":checked"))
        {
            $("#maxCarryDays").val("");
        }
    });
    // Initial page load setup
    toggleCarryForward();
    /**
     * Handles Leave Policy form submission.
     *
     * Validation Rules:
     * - Total Available Days must be greater than zero.
     * - Annual Leave requires Carry Forward to be checked.
     * - Max Carry Days must:
     *   - Be greater than zero.
     *   - Not exceed Total Available Days.
     *   - Not exceed 5 days.
     *
     * On successful validation:
     * - Sends AJAX POST request to create a Leave Policy.
     *
     * @param {Event} e Form submit event.
     * @returns {boolean|void}
     */
    $("#leaveForm").on("submit", function (e) {
        e.preventDefault();
        // Clear previous error messages
        $("#totalDaysError, #carryForwardError, #MaxCarryDayError").hide().text("");
        var selectedText = $("#LeaveType").find("option:selected").text().trim().toUpperCase();
        var isAnnual = (selectedText === "ANNUAL");
        var isChecked = $("#carryForward").is(":checked");
        var maxDays = parseInt($("#maxCarryDays").val());
        var totalDays = parseInt($("#Total_Days").val());
        // Validate Total Available Days
        if (isNaN(totalDays) || totalDays <= 0)
        {
            $("#totalDaysError").text("Total Available Day must be greater than 0.").show();
            return false;
        }
        // Annual Leave must have Carry Forward enabled
        if (isAnnual && !isChecked)
        {
            $("#carryForwardError").text("Please check 'Carry Forward' for Annual Leave.").show();
            return false;
        }
        // Validate Max Carry Days
        if (isChecked)
        {
            if (isNaN(maxDays) || maxDays <= 0)
            {
                $("#MaxCarryDayError").text("Please enter at least 1 day for 'Max Carry Days'.").show();
                return false;
            }
            else if (maxDays > totalDays)
            {
                $("#MaxCarryDayError").text("Max Carry Days cannot be greater than Total Available Days.").show();
                return false;
            }
            else if (maxDays > 5)
            {
                $("#MaxCarryDayError").text("Max Carry Days cannot exceed 5 days.").show();
                return false;
            }
        }
        // Submit form via AJAX
        $.ajax({ url: '/Leave/CreateLeavePolicy', type: 'POST', data: $(this).serialize(), success: function (response) {
                if (response.success)
                {
                    alert("Saved Successfully!");
                    location.reload();
                }
                else
                {
                    $("#totalDaysError").text(response.message).show();
                }
            },
            error: function (xhr) {
                console.log("Server Error:", xhr.responseText);
                $("#totalDaysError").text("System error. Please check console.").show();
            }
        });
    });
});

//-----------------------End Leave Policy-----------------------//