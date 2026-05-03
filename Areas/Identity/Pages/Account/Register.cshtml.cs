using System.ComponentModel.DataAnnotations;
using CampusConnect.Models;
using CampusConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusConnect.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [Compare("Password")]
            public string? ConfirmPassword { get; set; }
        }

        public void OnGet() { }

        // ✅ SEND OTP
        public async Task<IActionResult> OnPostSendOtp(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTPEmail", email);

            Console.WriteLine("OTP: " + otp);

            await _emailService.SendEmailAsync(
                email,
                "CampusConnect OTP",
                $@"
                    <div style='font-family:Segoe UI, Arial; padding:20px;'>

                        <h2 style='margin-bottom:5px;'>
                            <a href='https://localhost:7180' style='text-decoration:none; color:#2e7d32;'>
                                CampusConnect
                            </a>
                        </h2>

                        <p style='color:gray; font-size:14px;'>Secure Account Verification</p>

                        <hr/>

                        <p>Your One-Time Password (OTP) is:</p>

                        <div style='
                            font-size:28px;
                            font-weight:bold;
                            color:white;
                            background:#2e7d32;
                            padding:12px 20px;
                            display:inline-block;
                            border-radius:8px;
                            letter-spacing:3px;'>
                            {otp}
                        </div>

                        <p style='margin-top:20px;'>
                            This OTP is valid for a short time. Please do not share it with anyone.
                        </p>

                        <br/>

                        <p style='font-size:12px; color:gray;'>
                            If you did not request this, you can safely ignore this email.
                        </p>

                    </div>
                    "
            );

            return new JsonResult(new { success = true });
        }

        // ✅ VERIFY OTP
        public IActionResult OnPostVerifyOtp(string email, string otp)
        {
            var savedOtp = HttpContext.Session.GetString("OTP");
            var savedEmail = HttpContext.Session.GetString("OTPEmail");

            if (savedOtp == otp && savedEmail == email)
            {
                HttpContext.Session.SetString("OTPVerified", "true");
                HttpContext.Session.SetString("OTPVerifiedEmail", email); // 🔥 FIX

                return new JsonResult(new { success = true });
            }

            return new JsonResult(new { success = false });
        }

        // ✅ REGISTER
        public async Task<IActionResult> OnPostAsync()
        {
            var isVerified = HttpContext.Session.GetString("OTPVerified");
            var verifiedEmail = HttpContext.Session.GetString("OTPVerifiedEmail");

            if (isVerified != "true" || verifiedEmail != Input.Email)
            {
                ModelState.AddModelError("", "Please verify your email first.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    HttpContext.Session.Remove("OTP");
                    HttpContext.Session.Remove("OTPEmail");
                    HttpContext.Session.Remove("OTPVerified");
                    HttpContext.Session.Remove("OTPVerifiedEmail");

                    await _signInManager.SignInAsync(user, false);
                    return Redirect("/");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return Page();
        }
    }
}