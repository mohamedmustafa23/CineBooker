using System.ComponentModel.DataAnnotations;
using CineBooker.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineBooker.Models
{
    public class Booking
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        public int ShowId { get; set; }
        public Show Show { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public PaymentStatus StatusOfPayment { get; set; } = PaymentStatus.Pending;

        [Required]
        public DateTime BookedDate { get; set; } = DateTime.Now;
        public string? ConfirmationNumber { get; set; }
        public int NumberOfSeats { get; set; }

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
