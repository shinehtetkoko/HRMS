using HRMS.Models.Auth;
using HRMS.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HRMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        // GET: /Auth/ChangePassword
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

        // POST: /Auth/ChangePassword
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

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("HR")) return RedirectToAction("EmployeeDirectory", "Employee");
                return RedirectToAction("DailyCheckIn", "Attendance");
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            return View(new LoginViewModel());
        }

        // POST: /Auth/Login 
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

            // If first time login, navigate to ChangePassword
            if (result.IsFirstLogin)
            {
                TempData["UserEmail"] = result.Email; // Put Email into TempData 
                return RedirectToAction("ChangePassword");
            }

            string userRole = result.RoleName;

            // --- COOKIE AUTHENTICATION ---
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.User_Name),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Role, userRole)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (userRole.Equals("HR", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("EmployeeDirectory", "Employee");
            }

            return RedirectToAction("DailyCheckIn", "Attendance");

        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Auth/ForgotPassword
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

            // send email
            await _emailService.SendResetPasswordEmailAsync(email, resetLink ?? "");

            return Ok(new { success = true, message = "Reset link has been successfully sent to your email!" });
        }

        // GET: /Auth/ResetPassword
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

        // POST: /Auth/ResetPassword
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
    }
}