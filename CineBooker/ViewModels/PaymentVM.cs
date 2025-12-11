namespace CineBooker.ViewModels
{
    public class PaymentVM
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaDetails { get; set; } // اسم السينما والقاعة
        public DateTime ShowDate { get; set; }
        public string SeatNumbers { get; set; } // A1, A2, B5
        public int NumberOfSeats { get; set; }
        public decimal TotalAmount { get; set; }

        // وقت انتهاء حجز المقاعد (لأجل العد التنازلي)
        public DateTime LockExpiration { get; set; }
    }
}