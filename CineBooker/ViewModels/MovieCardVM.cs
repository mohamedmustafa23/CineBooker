namespace CineBooker.ViewModels
{
    public class MovieCardVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PosterUrl { get; set; }
        public string Genres { get; set; } // تجميع الأنواع كنص واحد (Action, Drama)
        public int Duration { get; set; }
        public double Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
