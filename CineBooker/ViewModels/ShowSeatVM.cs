using CineBooker.Models;
using CineBooker.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CineBooker.ViewModels
{
    public class ShowSeatVM
    {
        // Filters
        public int? ShowId { get; set; }
        public SeatStatus? Status { get; set; }

        // Data
        public Show? SelectedShow { get; set; }
        public List<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();

        // Lists for Dropdown
        public IEnumerable<SelectListItem> ShowList { get; set; }

        // Stats
        public int TotalSeats => ShowSeats.Count;
        public int BookedCount => ShowSeats.Count(s => s.Status == SeatStatus.Booked);
        public int LockedCount => ShowSeats.Count(s => s.Status == SeatStatus.Locked);
        public int AvailableCount => ShowSeats.Count(s => s.Status == SeatStatus.Available);
        public double OccupancyRate => TotalSeats > 0 ? Math.Round((double)BookedCount / TotalSeats * 100, 1) : 0;

        // Locked Seats Details (For Sidebar)
        public List<ShowSeat> LockedSeatsList => ShowSeats
            .Where(s => s.Status == SeatStatus.Locked)
            .OrderBy(s => s.LockExpiration)
            .ToList();
    }
}