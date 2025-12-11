namespace CineBooker.ViewModels
{
    public class CinemaShowtimeVM
    {
        public string CinemaName { get; set; }
        public string Address { get; set; }
        public List<ShowtimeVM> Shows { get; set; } = new List<ShowtimeVM>();
    }
}
