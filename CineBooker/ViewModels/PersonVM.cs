using CineBooker.Models;

namespace CineBooker.ViewModels
{
    public class PersonVM
    {
        public List<ApplicationUser> Persons { get; set; }
        public int TotalPerson { get; set; }
    }
}
