using Microsoft.AspNetCore.Mvc.Rendering;

namespace CineBooker.ViewModels
{
    public class MovieListVM
    {
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();

        // Filters (القيم المختارة)
        public string SearchTerm { get; set; }
        public int? GenreId { get; set; }
        public string Language { get; set; }
        public DateTime? FilterDate { get; set; }

        // السياق (هل نحن داخل سينما معينة؟)
        public int? CinemaId { get; set; }
        public string CinemaName { get; set; } // للـ Breadcrumb

        // Dropdowns Data (لتعبئة القوائم)
        public IEnumerable<SelectListItem> Genres { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }
    }
}
