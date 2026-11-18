
namespace BloodDonorApp.ViewModels
{
    public class DonorResultViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public int Age { get; set; }

        public string Gender { get; set; } = "";
        public string BloodGroup { get; set; } = "";
        public string State { get; set; } = "";

        public bool IsAvailable { get; set; }
    }
}
