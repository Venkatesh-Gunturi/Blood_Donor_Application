using System.Diagnostics;
using BloodDonorApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BloodDonorApp.Data;
using BloodDonorApp.ViewModels;
using BloodDonorApp.Services;

namespace BloodDonorApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly PinCodeValidationService _pinCodeService;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, PinCodeValidationService pinCodeService)
        {
            _logger = logger;
            _context = context;
            _pinCodeService = pinCodeService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
    string? bloodGroup,
    string? state,
    string? pinCode,
    int minAge = 18,
    int maxAge = 65,
    string? genderFilter = null,
    bool? hasTattooFilter = null)
        {
            var model = new DonorSearchViewModel
            {
                BloodGroup = bloodGroup,
                State = state,
                PinCode = pinCode,
                MinAge = minAge,
                MaxAge = maxAge,
                GenderFilter = genderFilter,
                HasTattooFilter = hasTattooFilter
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


            // Verify State and PIN Code before searching donors
            if (!string.IsNullOrWhiteSpace(pinCode))
            {
                var pinResult = await _pinCodeService.ValidateAsync(pinCode);

                // Invalid PIN or API failure
                if (!pinResult.IsValid)
                {
                    model.ErrorMessage = pinResult.Error
                        ?? "Unable to verify the PIN code.";

                    return View(model);
                }

                // Compare states only when a specific state is selected
                if (state != "All")
                {
                    bool stateMatches = pinResult.States.Any(s =>
                        string.Equals(
                            s,
                            state,
                            StringComparison.OrdinalIgnoreCase));

                    if (!stateMatches)
                    {
                        string actualStates =
                            string.Join(", ", pinResult.States);

                        model.ErrorMessage =
                            $"PIN code {pinCode} belongs to {actualStates}, " +
                            $"but you selected {state}. " +
                            "Please correct your search location.";

                        return View(model);
                    }
                }
            }


            if (minAge < 18 || maxAge > 65 || minAge > maxAge)
            {
                model.ErrorMessage = "Please select a valid age range between 18 and 65.";
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(genderFilter) &&
                genderFilter != "Male" &&
                genderFilter != "Female" &&
                genderFilter != "Other")
            {
                model.ErrorMessage = "Please select a valid gender filter.";
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


            // Filter by age using date of birth
            DateTime today = DateTime.Today;

            DateTime oldestAllowedBirthDate = today.AddYears(-(maxAge + 1)).AddDays(1);
            DateTime youngestAllowedBirthDate = today.AddYears(-minAge);

            query = query.Where(d =>
                d.DateOfBirth >= oldestAllowedBirthDate &&
                d.DateOfBirth <= youngestAllowedBirthDate);

            // Filter by gender
            if (!string.IsNullOrWhiteSpace(genderFilter))
            {
                query = query.Where(d => d.Gender == genderFilter);
            }

            // Filter by tattoo status
            if (hasTattooFilter.HasValue)
            {
                query = query.Where(d => d.HasTattoo == hasTattooFilter.Value);
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
                bool filtersApplied =
                    minAge != 18 ||
                    maxAge != 65 ||
                    !string.IsNullOrWhiteSpace(genderFilter) ||
                    hasTattooFilter.HasValue;

                if (filtersApplied)
                {
                    model.EmptyMessage =
                        "No registered donors match your selected filters. " +
                        "Try adjusting the age range, gender, or tattoo preference.";
                }
                else if (!string.IsNullOrWhiteSpace(pinCode))
                {
                    model.EmptyMessage = state == "All"
                        ? $"No {bloodGroup} donor records match PIN code {pinCode} in BloodDonorApp."
                        : $"No {bloodGroup} donor records match PIN code {pinCode}, {state}, in BloodDonorApp.";
                }
                else if (state == "All")
                {
                    model.EmptyMessage =
                        $"No {bloodGroup} donor records match your search in BloodDonorApp.";
                }
                else
                {
                    model.EmptyMessage =
                        $"No {bloodGroup} donor records match your search in {state} in BloodDonorApp.";
                }
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> DonorDetails(int id)
        {
            var donor = await _context.Donors
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.DateOfBirth,
                    d.Gender,
                    d.BloodGroup,
                    d.State,
                    d.City,
                    d.PinCode,
                    d.HasDisability,
                    d.HasTattoo,
                    d.Email,
                    d.LastDonationDate
                })
                .FirstOrDefaultAsync();

            if (donor == null)
            {
                return NotFound();
            }

            DateTime today = DateTime.Today;

            // Calculate age without exposing date of birth
            int age = today.Year - donor.DateOfBirth.Year;

            if (donor.DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            // 0 = Unknown
            // 1 = Potentially eligible
            // 2 = Recently donated
            int donationStatus = 0;

            if (donor.LastDonationDate.HasValue)
            {
                DateTime lastDonation = donor.LastDonationDate.Value.Date;

                if (lastDonation <= today &&
                    lastDonation.AddMonths(6) <= today)
                {
                    donationStatus = 1;
                }
                else
                {
                    donationStatus = 2;
                }
            }

            var model = new DonorDetailsViewModel
            {
                Id = donor.Id,
                FullName = $"{donor.FirstName} {donor.LastName}",
                Age = age,
                Gender = donor.Gender,
                BloodGroup = donor.BloodGroup,
                State = donor.State,
                City = donor.City,
                PinCode = donor.PinCode,
                HasDisability = donor.HasDisability,
                HasTattoo = donor.HasTattoo,
                Email = donor.Email,
                DonationStatus = donationStatus
            };

            return PartialView("_DonorDetails", model);
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
