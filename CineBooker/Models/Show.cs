using System.ComponentModel.DataAnnotations;

namespace CineBooker.Models
{
    public class Show
    {
        public int Id { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        [Required]
        public int CinemaHallId { get; set; }
        public CinemaHall CinemaHall { get; set; }


        public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
