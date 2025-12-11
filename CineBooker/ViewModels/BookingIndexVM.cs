using Microsoft.AspNetCore.Mvc.Rendering;
using CineBooker.Models;
using CineBooker.Enums;

namespace CineBooker.ViewModels
{
    public class BookingIndexVM
    {
        // Filters
        public string Search { get; set; }
        public int? MovieId { get; set; }
        public DateTime? Date { get; set; }
        public PaymentStatus? Status { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> MovieList { get; set; }

        // Data List
        public IEnumerable<Booking> Bookings { get; set; }

        // Statistics (Computed based on filtered data or all data)
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }
    }
}