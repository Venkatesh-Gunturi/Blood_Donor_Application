
using System;
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        public string? City { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "PIN code must contain exactly 6 digits.")]
        [Display(Name = "PIN Code")]
        public string PinCode { get; set; } = string.Empty;

        [Display(Name = "Do you have a disability?")]
        public bool HasDisability { get; set; }

        [Display(Name = "Do you have a tattoo?")]
        public bool HasTattoo { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Last Donation Date")]
        public DateTime? LastDonationDate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Mobile number must contain exactly 10 digits.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must contain at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "I understand that my email will be publicly accessible through View Contact.")]
        public bool AcceptPublicContact { get; set; }
    }
}
