namespace CineBooker.ViewModels
{
    public class DashboardVM
    {
        // 1. Top Cards Stats
        public int TotalMovies { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCinemas { get; set; }
        public int TotalHalls { get; set; }

        // Growth Percentages (مقارنة بالشهر الماضي)
        public double MoviesGrowth { get; set; }
        public double BookingsGrowth { get; set; }
        public double RevenueGrowth { get; set; }

        // 2. Charts Data
        public List<string> ChartLabels { get; set; } = new List<string>(); // أيام الأسبوع
        public List<int> ChartValues { get; set; } = new List<int>(); // عدد الحجوزات

        public int SeatsBooked { get; set; }
        public int SeatsAvailable { get; set; }
        public int SeatsLocked { get; set; }

        // 3. Tables Data
        public List<TopMovieStat> TopMovies { get; set; } = new List<TopMovieStat>();
        public IEnumerable<CineBooker.Models.Booking> RecentBookings { get; set; }
        public List<TodaysShowStat> TodaysShows { get; set; } = new List<TodaysShowStat>();

        // 4. User Stats
        public int AdminCount { get; set; }
        public int CustomerCount { get; set; }

        // 5. Quick Stats
        public int ActiveMoviesCount { get; set; }
        public int SeatsSoldToday { get; set; }
    }
}