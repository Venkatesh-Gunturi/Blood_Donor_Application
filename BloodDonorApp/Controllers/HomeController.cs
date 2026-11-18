using System.Diagnostics;
using BloodDonorApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BloodDonorApp.Data;
using BloodDonorApp.ViewModels;

namespace BloodDonorApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            string? bloodGroup,
            string? state,
            string? pinCode)
        {
            var model = new DonorSearchViewModel
            {
                BloodGroup = bloodGroup,
                State = state,
                PinCode = pinCode
            };

            // Initial page load
            if (bloodGroup == null &&
                state == null &&
                pinCode == null)
            {
                return View(model);
            }

            model.HasSearched = true;

            // Validate required inputs
            string[] validGroups =
            {
        "A+", "A-", "B+", "B-",
        "AB+", "AB-", "O+", "O-",
        "Oh (Bombay)"
    };

            if (string.IsNullOrWhiteSpace(bloodGroup) ||
                !validGroups.Contains(bloodGroup))
            {
                model.ErrorMessage = "Please select a valid blood group.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                model.ErrorMessage = "Please select a State / UT.";
                return View(model);
            }

            // Validate PIN format
            if (!string.IsNullOrWhiteSpace(pinCode) &&
                !System.Text.RegularExpressions.Regex.IsMatch(
                    pinCode, @"^[0-9]{6}$"))
            {
                model.ErrorMessage = "Enter a valid 6-digit PIN code.";
                return View(model);
            }

            // Build database query
            var query = _context.Donors
                .AsNoTracking()
                .Where(d => d.BloodGroup == bloodGroup);

            if (state != "All")
            {
                query = query.Where(d => d.State == state);
            }

            if (!string.IsNullOrWhiteSpace(pinCode))
            {
                query = query.Where(d => d.PinCode == pinCode);
            }

            // Fetch only fields needed for public results
            var donors = await query
                .Select(d => new
                {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.DateOfBirth,
                    d.Gender,
                    d.BloodGroup,
                    d.State,
                    d.LastDonationDate
                })
                .ToListAsync();

            DateTime today = DateTime.Today;

            // Convert database results into public view models
            model.Donors = donors.Select(d =>
            {
                int age = today.Year - d.DateOfBirth.Year;

                if (d.DateOfBirth.Date > today.AddYears(-age))
                {
                    age--;
                }

                bool isAvailable =
                    !d.LastDonationDate.HasValue ||
                    d.LastDonationDate.Value.Date.AddMonths(6) <= today;

                return new DonorResultViewModel
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Age = age,
                    Gender = d.Gender,
                    BloodGroup = d.BloodGroup,
                    State = d.State,
                    IsAvailable = isAvailable
                };
            })
            .OrderByDescending(d => d.IsAvailable)
            .ThenBy(d => d.FirstName)
            .ToList();

            // Count results
            model.AvailableCount =
                model.Donors.Count(d => d.IsAvailable);

            model.UnavailableCount =
                model.Donors.Count(d => !d.IsAvailable);

            // Handle empty search results
            if (model.Donors.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(pinCode))
                {
                    model.EmptyMessage = state == "All"
                        ? $"No {bloodGroup} donor records are currently available for PIN code {pinCode} in BloodDonorApp."
                        : $"No {bloodGroup} donor records are currently available for PIN code {pinCode}, {state}, in BloodDonorApp.";
                }
                else if (state == "All")
                {
                    model.EmptyMessage =
                        $"No {bloodGroup} donor records are currently available in BloodDonorApp.";
                }
                else
                {
                    model.EmptyMessage =
                        $"No {bloodGroup} donor records are currently available for {state} in BloodDonorApp.";
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult LearnMore()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
