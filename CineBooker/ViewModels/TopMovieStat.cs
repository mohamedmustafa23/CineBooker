namespace CineBooker.ViewModels
{
    public class TopMovieStat
    {
        public string Title { get; set; }
        public string PosterUrl { get; set; }
        public double Rating { get; set; }
        public int BookingsCount { get; set; }
        public decimal Revenue { get; set; }
    }
}