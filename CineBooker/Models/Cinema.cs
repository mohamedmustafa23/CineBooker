using System.ComponentModel.DataAnnotations;

namespace CineBooker.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? LogoUrl { get; set; } 
        [Required]
        public int AddressId { get; set; }
        public Address Address { get; set; }
        public string AddressLine { get; set; }
        public ICollection<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();
    }
}
