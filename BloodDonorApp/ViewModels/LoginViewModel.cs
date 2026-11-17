
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or mobile number is required.")]
        [Display(Name = "Email or Mobile Number")]
        public string LoginIdentifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
