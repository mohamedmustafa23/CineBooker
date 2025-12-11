namespace CineBooker.ViewModels
{
    public class MovieDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public int Duration { get; set; }
        public double Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Language { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public string Director { get; set; }
        public List<string> Cast { get; set; } = new List<string>();

        // قائمة التواريخ المتاحة (للـ Tabs)
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public DateTime SelectedDate { get; set; }
    }
}
