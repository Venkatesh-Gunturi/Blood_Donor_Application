
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApp.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; } = string.Empty;

        public string? City { get; set; }

        [Required(ErrorMessage = "PIN code is required.")]
        [RegularExpression(@"^[0-9]{6}$",
            ErrorMessage = "Enter a valid 6-digit PIN code.")]
        public string PinCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        public bool HasDisability { get; set; }
        public bool HasTattoo { get; set; }
    }
}
