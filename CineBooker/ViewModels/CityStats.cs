namespace CineBooker.ViewModels
{
    public class CityStatsVM
    {
        public string Name { get; set; }
        public int CinemaCount { get; set; }
        public string ImageUrl { get; set; } // سنأخذها من Address.PhotoUrl
    }
}
