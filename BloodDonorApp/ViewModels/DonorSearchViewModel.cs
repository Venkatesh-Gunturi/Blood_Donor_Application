
using System.Collections.Generic;

namespace BloodDonorApp.ViewModels
{
    public class DonorSearchViewModel
    {
        // SEARCH INPUTS
        public string? BloodGroup { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }

        // WHETHER A SEARCH WAS SUBMITTED
        public bool HasSearched { get; set; }

        // MATCHING DONORS
        public List<DonorResultViewModel> Donors { get; set; } = new();

        // SEARCH / VALIDATION MESSAGES
        public string? ErrorMessage { get; set; }
        public string? EmptyMessage { get; set; }

        // RESULT COUNTS
        public int AvailableCount { get; set; }
        public int UnavailableCount { get; set; }

        public int MinAge { get; set; } = 18;

        public int MaxAge { get; set; } = 65;

        public string? GenderFilter { get; set; }

        public bool? HasTattooFilter { get; set; }
    }
}
