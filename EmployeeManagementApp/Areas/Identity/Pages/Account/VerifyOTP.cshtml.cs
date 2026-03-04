using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.Areas.Identity.Pages.Account
{
    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public VerifyOTPModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter the 6-digit code.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        public string OtpCode { get; set; }

        public string ReturnUrl { get; set; }

        public void OnGet(string email, string returnUrl = null)
        {
            Email = email;
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to load user.");
                return Page();
            }

            // Grab the OTP we securely saved in the database
            var expectedCode = await _userManager.GetAuthenticationTokenAsync(user, "Default", "EmailOTP");

            if (expectedCode == OtpCode)
            {
                // SUCCESS! The codes match. Confirm their email.
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                // Log them in!
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Optional: Delete the token so it can't be used again
                await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "EmailOTP");

                return LocalRedirect(returnUrl);
            }

            // If the code is wrong, show an error
            ModelState.AddModelError(string.Empty, "Invalid verification code. Please try again.");
            return Page();
        }
    }
}