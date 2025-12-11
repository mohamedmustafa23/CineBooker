using System.ComponentModel.DataAnnotations;

namespace CineBooker.Models
{
    public class BookingSeat
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int ShowSeatId { get; set; }

        public Booking Booking { get; set; }
        public ShowSeat ShowSeat { get; set; }
    }
}
