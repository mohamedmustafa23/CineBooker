using Microsoft.AspNetCore.Mvc.Rendering;

namespace CineBooker.ViewModels
{
    public class SeatVM
    {
        public int? CinemaId { get; set; }
        public int? HallId { get; set; }

        public IEnumerable<SelectListItem> CinemaList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> HallList { get; set; } = new List<SelectListItem>();

        public CinemaHall? SelectedHall { get; set; }
        public List<Seat> Seats { get; set; } = new List<Seat>();

        public int MaxRows { get; set; }
        public int MaxCols { get; set; }

        public string NewSeatRow { get; set; } 
        public int NewSeatCol { get; set; }
    }
}