
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApp.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        public int DonorId { get; set; }

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? UsedAtUtc { get; set; }
    }
}

