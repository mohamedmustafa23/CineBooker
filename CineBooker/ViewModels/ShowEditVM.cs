using System.ComponentModel.DataAnnotations;

namespace CineBooker.ViewModels
{
    public class ShowEditVM
    {
        public int Id { get; set; }

        // Read-only info
        public string MovieTitle { get; set; }
        public int MovieDuration { get; set; }
        public string CinemaName { get; set; }
        public string HallName { get; set; }

        // Editable
        [Required]
        [DataType(DataType.Date)]
        public DateTime ShowDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(1, 10000)]
        public int BasePrice { get; set; } // السعر الحالي للتذكرة

        // Stats
        public int TotalSeats { get; set; }
        public int BookedCount { get; set; } // بناء على Booking.NumberOfSeats
        public decimal TotalRevenue { get; set; } // Booking.Amount
    }
}