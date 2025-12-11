using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CineBooker.ViewModels
{
    public class ShowVM
    {
        [Required]
        [Display(Name = "Movie")]
        public int MovieId { get; set; }

        [Required]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        [Required]
        [Display(Name = "Hall")]
        public int CinemaHallId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ShowDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Ticket Price (Integers only)")]
        public int BasePrice { get; set; } = 50; // لأن ShowSeat.Price نوعه int

        // قوائم
        [ValidateNever]
        public IEnumerable<SelectListItem> MovieList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> CinemaList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> HallList { get; set; }
    }
}