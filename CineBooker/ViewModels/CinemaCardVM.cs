namespace CineBooker.ViewModels
{
    public class CinemaCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int HallCount { get; set; }
        public string? ImageUrl { get; set; }

        // يمكن إضافة المزيد من الخصائص لاحقاً مثل التقييم أو الميزات
        public double Rating { get; set; } = 4.5; // قيمة افتراضية
        public List<string> Features { get; set; } = new List<string> { "3D", "IMAX", "Parking" }; // قيم افتراضية
    }
}
