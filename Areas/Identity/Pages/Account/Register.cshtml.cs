
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Traffic.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins
        { get; set; } = new List<AuthenticationScheme>();

        public class InputModel
        {
            [Required(ErrorMessage = "E-posta adresi zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            [Display(Name = "E-posta Adresi")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Þifre zorunludur.")]
            [StringLength(
                100,
                MinimumLength = 8,
                ErrorMessage = "Þifre en az 8, en fazla 100 karakter olmalýdýr.")]
            [DataType(DataType.Password)]
            [Display(Name = "Þifre")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Þifre tekrarý zorunludur.")]
            [DataType(DataType.Password)]
            [Display(Name = "Þifre Tekrarý")]
            [Compare(
                nameof(Password),
                ErrorMessage = "Þifreler eþleþmiyor.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        // GET: /Identity/Account/Register
        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();
        }

        // POST: /Identity/Account/Register
        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ReturnUrl = returnUrl;

            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string email = Input.Email.Trim();

            // Ayný e-posta ile kayýt var mý?
            var existingUser = await _userManager
                .FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(Input.Email),
                    "Bu e-posta adresi zaten kayýtlý.");

                return Page();
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false
            };

            // Identity parola kurallarýný uygulayarak kullanýcý oluþtur.
            var result = await _userManager
                .CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Burada herhangi bir rol atamýyoruz.
                // Roller yalnýzca Admin panelinden atanýr.

                // Projede RequireConfirmedAccount=false olduðu için
                // kullanýcý kayýt sonrasý giriþ yapabilir.
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage(
                        "RegisterConfirmation",
                        new
                        {
                            email = email,
                            returnUrl = returnUrl
                        });
                }

                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }
    }
}
