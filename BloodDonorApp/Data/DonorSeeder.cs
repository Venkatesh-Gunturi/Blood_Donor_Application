
using BloodDonorApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodDonorApp.Data
{
    public static class DonorSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Do not insert the same test records twice.
            bool alreadySeeded = await context.Donors
                .AnyAsync(d => d.Email == "testdonor001@example.com");

            if (alreadySeeded)
                return;

            string[] firstNames =
            {
                "Arjun", "Priya", "Rahul", "Sneha", "Kiran",
                "Anjali", "Ravi", "Divya", "Vikram", "Meera",
                "Suresh", "Kavya", "Aditya", "Pooja", "Nikhil",
                "Swathi", "Rohit", "Anusha", "Manoj", "Deepa"
            };

            string[] lastNames =
            {
                "Sharma", "Reddy", "Kumar", "Patel", "Rao",
                "Naidu", "Singh", "Verma", "Gupta", "Das"
            };

            string[] bloodGroups =
            {
                "A+", "A-", "B+", "B-", "AB+",
                "AB-", "O+", "O-", "Oh (Bombay)"
            };

            // State, city and matching PIN code.
            var locations = new (string State, string City, string PinCode)[]
            {
                ("Telangana", "Hyderabad", "500072"),
                ("Andhra Pradesh", "Vijayawada", "520001"),
                ("Karnataka", "Bengaluru", "560001"),
                ("Tamil Nadu", "Chennai", "600001"),
                ("Kerala", "Thiruvananthapuram", "695001"),
                ("Maharashtra", "Mumbai", "400001"),
                ("Delhi", "New Delhi", "110001"),
                ("West Bengal", "Kolkata", "700001"),
                ("Gujarat", "Ahmedabad", "380001"),
                ("Rajasthan", "Jaipur", "302001")
            };

            var random = new Random(2026);
            var donors = new List<Donor>();
            var passwordHasher = new PasswordHasher<Donor>();

            for (int i = 1; i <= 100; i++)
            {
                var location = locations[(i - 1) % locations.Length];

                int age = random.Next(18, 66);

                var donor = new Donor
                {
                    FirstName = firstNames[random.Next(firstNames.Length)],
                    LastName = lastNames[random.Next(lastNames.Length)],
                    DateOfBirth = DateTime.Today
                        .AddYears(-age)
                        .AddDays(-random.Next(0, 300)),

                    Gender = i % 10 == 0
                        ? "Other"
                        : i % 2 == 0 ? "Female" : "Male",

                    BloodGroup = bloodGroups[(i - 1) % bloodGroups.Length],

                    State = location.State,
                    City = location.City,
                    PinCode = location.PinCode,

                    HasDisability = i % 7 == 0,
                    HasTattoo = i % 4 == 0,

                    LastDonationDate = i % 3 == 0
                        ? null
                        : i % 2 == 0
                            ? DateTime.Today.AddMonths(-2)
                            : DateTime.Today.AddMonths(-9),

                    Email = $"testdonor{i:D3}@example.com",
                    MobileNumber = $"90000{i:D5}"
                };

                donor.PasswordHash = passwordHasher.HashPassword(
                    donor,
                    $"TestDonor@{i:D3}");

                donors.Add(donor);
            }

            await context.Donors.AddRangeAsync(donors);
            await context.SaveChangesAsync();
        }
    }
}
