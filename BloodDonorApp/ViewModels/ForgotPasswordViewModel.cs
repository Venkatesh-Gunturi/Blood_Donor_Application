
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;
    }
}

