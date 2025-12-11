using System.ComponentModel.DataAnnotations;

namespace CineBooker.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Bio { get; set; }
        [Required]
        public string PhotoUrl { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
