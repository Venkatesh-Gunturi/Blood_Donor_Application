using System;
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApp.Models
{
    public class Donor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)]
        public string BloodGroup { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? City { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$")]
        public string PinCode { get; set; } = string.Empty;

        public bool HasDisability { get; set; }

        public bool HasTattoo { get; set; }

        public DateTime? LastDonationDate { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
}