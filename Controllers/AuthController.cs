using HRMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class AuthController : Controller
    {
        /***
         * This action handles the login form submission. It checks if the model state is valid and then retrieves the email and password from the input model. 
         * You can add your authentication logic here to verify the user's credentials and log them in.
        */
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /***
        * This action handles the routing for your forgot password page. It simply returns the view for the forgot password page.
        */
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /***
        * This action handles the routing for your change password page. It simply returns the view for the change password page.
        */
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

    }
}
