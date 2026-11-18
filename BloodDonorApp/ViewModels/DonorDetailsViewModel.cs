
namespace BloodDonorApp.ViewModels
{
    public class DonorDetailsViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string BloodGroup { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string? City { get; set; }

        public string PinCode { get; set; } = string.Empty;

        public bool HasDisability { get; set; }

        public bool HasTattoo { get; set; }

        public string Email { get; set; } = string.Empty;

        // 0 = Unknown, 1 = Potentially eligible,
        // 2 = Recently donated
        public int DonationStatus { get; set; }
    }
}
