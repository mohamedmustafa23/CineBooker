using CineBooker.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineBooker.Models
{
    public class ShowSeat
    {
        public int Id { get; set; }
        [Required]
        public int Price { get; set; }
        public SeatStatus Status { get; set; } = SeatStatus.Available;
        public DateTime? LockExpiration { get; set; }
        [Required]
        public int ShowId { get; set; }
        public Show Show { get; set; }
        [Required]
        public int SeatId { get; set; }
        public Seat Seat { get; set; }
    }
}
