//---------------------------Start Holiday Setup-----------------------------//
document.addEventListener("DOMContentLoaded", function () {
    const holidayTypeSelect = document.getElementById('Holiday_Type');
    const startDateInput = document.getElementById('Start_Date');
    const endDateInput = document.getElementById('End_Date');

    if (!holidayTypeSelect || !startDateInput || !endDateInput) return;

    /**
     * Synchronizes the start and end dates for single-day holidays.
     * Disables the end date input when "SingleDay" is selected.
     * @function handleDateLogic
     * @returns {void}
     */
    function handleDateLogic() {
        if (holidayTypeSelect.value === "SingleDay") {
            if (startDateInput.value) {
                endDateInput.value = startDateInput.value;
            }
            endDateInput.readOnly = true;
            endDateInput.style.backgroundColor = "#e9ecef";
        } else {
            endDateInput.readOnly = false;
            endDateInput.style.backgroundColor = "";
        }
    }
    holidayTypeSelect.addEventListener("change", handleDateLogic);
    startDateInput.addEventListener("change", handleDateLogic);
    handleDateLogic();
});
//---------------------------End Holiday Setup-----------------------------//


