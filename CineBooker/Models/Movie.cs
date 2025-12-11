using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineBooker.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Required]
        [Range(1, 500)]
        public int DurationInMinutes { get; set; }
        [StringLength(50)]
        public string? Language { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Rating { get; set; }
        public string? Director { get; set; }
        public string PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        [Range(0, 10)]
        [Column(TypeName = "decimal(3, 1)")]
        public decimal? AverageRating { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Show> Shows { get; set; } = new List<Show>();
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
}
