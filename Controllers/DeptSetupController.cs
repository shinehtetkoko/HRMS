using HRMS.Data.Entities;
using HRMS.Interfaces;    
using Microsoft.AspNetCore.Mvc;
public class DeptSetupController : Controller
{
    private readonly IDepartmentService _departmentService;
    /// <summary>
    ///Initializes DeptSetupController
    /// </summary>
    /// <param name="departmentService">Department service instance</param>
    public DeptSetupController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Loads the department setup page
    /// </summary>
    /// <returns>List of departments</returns>
    public async Task<IActionResult> DeptSetup()
    {
        var depts = await _departmentService.GetAllDepartmentsAsync();
        return View(depts);
    }

    /// <summary>
    /// Adds a new department.
    /// </summary>
    /// <param name="department">Department entity</param>
    /// <returns>JSON result</returns>
    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] Department department)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid data" });
        }

        try
        {
            await _departmentService.AddDepartmentAsync(department);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    /// <summary>
    /// Loads the edit view for a department
    /// </summary>
    /// <param name="department">Department entity</param>
    /// <returns>JSON result</returns>
    public async Task<IActionResult> Edit(int id)
    {
        var dept = await _departmentService.GetDepartmentByIdAsync(id);
        if (dept == null)
        {
            return NotFound();
        }
        return View(dept);
    }

    /// <summary>
    /// Saves updated department changes
    /// </summary>
    /// /// <param name="department">Updated department entity</param>
    /// <returns>Redirects to the DeptSetup page on success, or returns the view with errors.</returns>
    [HttpPost]
    public async Task<IActionResult> Edit(Department department)
    {  
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        try
        {
            await _departmentService.UpdateDepartmentAsync(department);
            return RedirectToAction(nameof(DeptSetup));
        }
        catch (KeyNotFoundException)
        { 
            return NotFound();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
            return View(department);
        }
    }

    /// <summary>
    /// Deletes a department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Redirects to the DeptSetup page after deletion.</returns>
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteDepartmentAsync(id);
        return RedirectToAction(nameof(DeptSetup));
    }
}