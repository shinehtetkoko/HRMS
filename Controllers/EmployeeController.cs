
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("DailyCheckIn", "Attendance");
            //return View();
        }

        // This action handles the routing for your setup page
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult ReviewRequests()
        {
            return View();
        }

        public IActionResult EmployeeDirectory()
        {
            // Views/Employee/EmployeeDirectory.cshtml 
            return View();
        }

    }
}