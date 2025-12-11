
namespace CineBooker.ViewModels
{
    public class CinemaListVM
    {
        public string CityName { get; set; } = "All Cities";
        public List<CinemaCardVM> Cinemas { get; set; } = new List<CinemaCardVM>();
    }

}
