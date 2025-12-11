using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CineBooker.ViewModels
{ 
    public class CinemaVM
    {
        public Cinema Cinema { get; set; }

        [Display(Name = "Select Address")]
        public int SelectedAddressId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> AddressList { get; set; }

        public IFormFile? CinemaLogo { get; set; }
    }
}
