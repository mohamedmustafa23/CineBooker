namespace CineBooker.ViewModels
{
    public class ShowtimeVM
    {
        public int ShowId { get; set; }
        public DateTime StartTime { get; set; }
        public string HallName { get; set; }
        public string Status { get; set; } // Available, Full, Filling Fast
        public string StatusColorClass { get; set; } // text-success, text-danger...
        public bool IsFull { get; set; }
    }
}
