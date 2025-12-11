using CineBooker.Enums;

namespace CineBooker.ViewModels
{
    public class BookTicketVM
    {
        public int ShowId { get; set; }
        public string MovieTitle { get; set; }
        public string PosterUrl { get; set; }
        public string CinemaName { get; set; }
        public string HallName { get; set; }
        public DateTime ShowDate { get; set; }

        // قائمة المقاعد للعرض
        public List<SeatMapItem> Seats { get; set; } = new List<SeatMapItem>();

        // القائمة التي سيتم إرسالها عند الحجز (IDs فقط)
        public List<int> SelectedShowSeatIds { get; set; } = new List<int>();
    }

    public class SeatMapItem
    {
        public int ShowSeatId { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string SeatName { get; set; } // مثال: A1, B5
        public decimal Price { get; set; }
        public SeatStatus Status { get; set; } // Available, Locked, Booked
    }
}