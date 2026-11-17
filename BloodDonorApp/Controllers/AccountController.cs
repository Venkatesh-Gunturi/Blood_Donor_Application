
using BloodDonorApp.Data;
using BloodDonorApp.Models;
using BloodDonorApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BloodDonorApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate donor age (18 to 65 years)
            if (model.DateOfBirth.HasValue)
            {
                DateTime today = DateTime.Today;
                DateTime dob = model.DateOfBirth.Value.Date;

                // Must be at least 18, and must not have turned 67 yet
                if (dob > today.AddYears(-18) ||
                    dob <= today.AddYears(-67))
                {
                    ModelState.AddModelError(
                        nameof(model.DateOfBirth),
                        "People below 18 or above 66 years of age are not allowed to donate blood.");
                }
            }

            // Step 1: Validate the registration form
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step 2: Check for an existing email address
            string email = model.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.Donors
                .AnyAsync(d => d.Email.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email address is already registered.");

                return View(model);
            }

            // Step 3: Check for an existing mobile number
            string mobile = model.MobileNumber.Trim();

            bool mobileExists = await _context.Donors
                .AnyAsync(d => d.MobileNumber == mobile);

            if (mobileExists)
            {
                ModelState.AddModelError(
                    nameof(model.MobileNumber),
                    "This mobile number is already registered.");

                return View(model);
            }

            // Step 4: Create a new donor object
            var donor = new Donor
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                DateOfBirth = model.DateOfBirth!.Value,
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                State = model.State,
                City = model.City?.Trim(),
                PinCode = model.PinCode,
                HasDisability = model.HasDisability,
                HasTattoo = model.HasTattoo,
                LastDonationDate = model.LastDonationDate,
                Email = email,
                MobileNumber = mobile
            };

            if (!model.AcceptPublicContact)
            {
                ModelState.AddModelError(
                    nameof(model.AcceptPublicContact),
                    "You must agree before registering.");
            }

            // Step 5: Hash the password before saving
            var passwordHasher = new PasswordHasher<Donor>();

            donor.PasswordHash = passwordHasher.HashPassword(
                donor,
                model.Password);

            // Step 6: Save the donor to SQL Server
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            // Step 7: Redirect after successful registration
            TempData["SuccessMessage"] =
                "Registration completed successfully!";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identifier = model.LoginIdentifier.Trim();

            var donor = await _context.Donors
                .FirstOrDefaultAsync(d =>
                    d.Email == identifier ||
                    d.MobileNumber == identifier);

            if (donor == null)
            {
                ModelState.AddModelError(
                    nameof(model.LoginIdentifier),
                    "No account found with this email or mobile number.");

                return View(model);
            }

            var passwordHasher = new PasswordHasher<Donor>();

            var result = passwordHasher.VerifyHashedPassword(
                donor,
                donor.PasswordHash,
                model.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    nameof(model.Password),
                    "Invalid password.");

                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, donor.Id.ToString()),
        new Claim(ClaimTypes.Name, donor.FirstName),
        new Claim(ClaimTypes.Email, donor.Email)
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                }
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }
    }
}
