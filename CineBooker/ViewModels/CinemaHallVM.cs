using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CineBooker.ViewModels
{
    public class CinemaHallVM
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CinemaList { get; set; }

        [Required]
        [Range(1, 30, ErrorMessage = "Rows must be between 1 and 30")]
        public int Rows { get; set; } = 10;

        [Required]
        [Range(1, 50, ErrorMessage = "Columns must be between 1 and 50")]
        public int Columns { get; set; } = 15;
    }
}