using System.ComponentModel.DataAnnotations;

namespace CineBooker.Models
{
    public class Address
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(3)]
        public string City { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public virtual ICollection<Cinema> Cinemas { get; set; } = new List<Cinema>();
    }
}
