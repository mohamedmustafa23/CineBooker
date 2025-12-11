using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations;

namespace CineBooker.Models
{
    public class MovieGenre
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int GenreId { get; set; }

        public Movie Movie { get; set; }
        public Genre Genre { get; set; }
    }
}
