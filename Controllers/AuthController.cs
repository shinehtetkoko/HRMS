using HRMS.Models.Auth;
using HRMS.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HRMS.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        #region Password Management (First Login)
        /// <summary>
        /// Displays the change password UI view layout specifically enforced during user's first login stage.
        /// </summary>
        /// <param name="source">An optional tracker tag monitoring the route entry origin point.</param>
        /// <returns>The ChangePassword view template populated with user email references.</returns>
        [HttpGet]
        public IActionResult ChangePassword([FromQuery] string source)
        {
            if (TempData["UserEmail"] == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserEmail = TempData["UserEmail"].ToString();
            TempData.Keep("UserEmail");

            ViewData["Source"] = source;
            return View(new ChangePasswordViewModel { Email = ViewBag.UserEmail });
        }

        /// <summary>
        /// Commits the newly declared password payload into the system to replace the initial automated OTP setup.
        /// </summary>
        /// <param name="model">The configuration data model containing new and confirmed password pairs.</param>
        /// <returns>Redirects back to the login page on success, or stays on the view to surface validation alerts.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserEmail = model.Email;
                return View(model);
            }

            var result = await _authService.ChangePasswordAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Password updated successfully! Please login with your new password.";
                return RedirectToAction("Login");
            }

            ViewBag.ErrorMessage = result.Message;
            ViewBag.UserEmail = model.Email;
            return View(model);
        }
        #endregion

        #region Login and Logout
        /// <summary>
        /// Displays the Login UI page if the user is not authenticated; otherwise redirects to their designated home dashboard.
        /// </summary>
        /// <returns>The Login view layout, or a redirect action based on the authenticated user's role.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("AdminDashboard", "Admin");
                if (User.IsInRole("HR")) return RedirectToAction("HRDashboard", "Employee");
                return RedirectToAction("DailyCheckIn", "Attendance");
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            return View(new LoginViewModel());
        }

        /// <summary>
        /// Validates user credentials, handles first-time login enforcement, and issues the Cookie authentication state upon success.
        /// </summary>
        /// <param name="model">The credentials payload including email and password entries.</param>
        /// <returns>Redirects to proper dashboard layout, or forces password changes on first time logins.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ValidateLoginAsync(model.Input.Email, model.Input.Password);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            if (result.IsFirstLogin)
            {
                TempData["UserEmail"] = result.Email; 
                return RedirectToAction("ChangePassword");
            }

            string userRole = result.RoleName;

            // --- COOKIE AUTHENTICATION ---
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.User_Name),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Role, userRole),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else if (userRole.Equals("HR", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("HRDashboard", "Employee");
            }

            return RedirectToAction("DailyCheckIn", "Attendance");

        }

        /// <summary>
        /// Clears out the user's browser active Cookie authentication sessions and safely logs them out.
        /// </summary>
        /// <returns>Redirects back to the Login landing page layout.</returns>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region Forgot and Reset Password (Recovery)
        /// <summary>
        /// Displays the Forgot Password form layout page to start account recovery processes.
        /// </summary>
        /// <returns>The basic Forgot Password view layout.</returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Generates an authentication recovery token link and transmits it directly via EmailServices to users.
        /// </summary>
        /// <param name="email">The target destination user email account address to request resets.</param>
        /// <returns>A 200 OK status on success, or a 400 BadRequest if email accounts are unrecognized.</returns>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            var result = await _authService.VerifyForgotPasswordAsync(email);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            var resetLink = Url.Action("ResetPassword", "Auth",
                new { token = result.Token, email = email }, Request.Scheme);

            await _emailService.SendResetPasswordEmailAsync(email, resetLink ?? "");

            return Ok(new { success = true, message = "Reset link has been successfully sent to your email!" });
        }

        /// <summary>
        /// Evaluates incoming password recovery requests tokens validity prior to exposing entry interface view panels.
        /// </summary>
        /// <param name="token">The generated security verification hash identifier.</param>
        /// <param name="email">The target user account email address tied directly to the requested token.</param>
        /// <returns>The unified ChangePassword entry interface, or error logs if security tokens expired.</returns>
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var isValid = await _authService.VerifyResetTokenAsync(email, token);
            if (!isValid)
            {
                return Content("This password reset link is invalid or has expired. Please request a new one.");
            }

            ViewData["Source"] = "forgot";
            var model = new ChangePasswordViewModel { Token = token, Email = email };
            return View("ChangePassword", model);
        }

        /// <summary>
        /// Finalizes the account recovery lifecycle by safely updating the database using verified token structures.
        /// </summary>
        /// <param name="model">The recovery change password view model schema configuration payload details.</param>
        /// <returns>Redirects back onto the login portal layout upon success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Source"] = "forgot";
                return View("ChangePassword", model);
            }

            var result = await _authService.ResetPasswordAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully! Please login with your new password.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            ViewData["Source"] = "forgot";
            return View("ChangePassword", model);
        }
        #endregion
    }
}