namespace CineBooker.ViewModels
{
    public class MovieVM
    {
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
        public Movie? movie { get; set; }

    }
}
