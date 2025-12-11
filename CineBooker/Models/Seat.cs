using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineBooker.Models
{
    public class Seat
    {
        public int Id { get; set; }
        [Required]
        public int SeatRow { get; set; }
        [Required]
        public int SeatCol { get; set; }

        public int CinemaHallId { get; set; }
        public CinemaHall CinemaHall { get; set; }

        public virtual ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();
    }
}
