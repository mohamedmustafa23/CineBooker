using System.ComponentModel.DataAnnotations;

namespace CineBooker.ViewModels
{
    public class UserProfileVM
    {
        [Display(Name = "First Name")]
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        public string Email { get; set; } 

        public string Address { get; set; }

        public string Gender { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string ProfilePictureUrl { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }

        public List<UserBookingSummary> Bookings { get; set; } = new List<UserBookingSummary>();
        public List<UserBookingSummary> UpcomingBookings { get; set; } = new List<UserBookingSummary>();
        public List<UserBookingSummary> PastBookings { get; set; } = new List<UserBookingSummary>();
    }

    public class UserBookingSummary
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; }
        public string PosterUrl { get; set; }
        public string CinemaName { get; set; }
        public DateTime ShowDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}