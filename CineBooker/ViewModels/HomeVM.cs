namespace CineBooker.ViewModels
{
    public class HomeVM
    {
        public List<CityStatsVM> Cities { get; set; } = new List<CityStatsVM>();
        public List<MovieCardVM> NowShowingMovies { get; set; } = new List<MovieCardVM>();
        public List<MovieCardVM> UpcomingMovies { get; set; } = new List<MovieCardVM>();
    }
}