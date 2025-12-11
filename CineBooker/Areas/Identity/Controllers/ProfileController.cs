using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace CineBooker.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<Booking> _bookingRepo;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IRepository<Booking> bookingRepo)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _bookingRepo = bookingRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            // 1. جلب كل الحجوزات
            var allBookings = await GetUserBookings(user.Id);

            // 2. تقسيم الحجوزات
            var now = DateTime.UtcNow; // أو حسب توقيت بلدك

            var model = new UserProfileVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                // ... باقي البيانات
                ProfilePictureUrl = user.ProfilePicture,

                // الحجوزات القادمة: تاريخ العرض لم يأتِ بعد، وحالة الدفع مقبولة
                UpcomingBookings = allBookings
                    .Where(b => b.ShowDate > now && b.Status == "Approved")
                    .OrderBy(b => b.ShowDate) // الأقرب فالأبعد
                    .ToList(),

                // الحجوزات السابقة: تاريخ العرض فات، أو تم إلغاؤها
                PastBookings = allBookings
                    .Where(b => b.ShowDate <= now || b.Status != "Approved")
                    .OrderByDescending(b => b.ShowDate) // الأحدث فالأقدم
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserProfileVM model, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;

            if (model.ProfilePictureFile != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Profiles");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePictureFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);
                using (var Stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePictureFile.CopyToAsync(Stream, cancellationToken);
                }

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }
                user.ProfilePicture = "/images/Profiles/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            model.Bookings = await GetUserBookings(user.Id);
            return View("Index", model);
        }

        private async Task<List<UserBookingSummary>> GetUserBookings(string userId)
        {
            var bookings = await _bookingRepo.GetAsync(
                b => b.UserId == userId,
                include: src => src.Include(b => b.Show).ThenInclude(s => s.Movie)
                                   .Include(b => b.Show).ThenInclude(s => s.CinemaHall).ThenInclude(c => c.Cinema)
            );

            return bookings.OrderByDescending(b => b.BookedDate).Select(b => new UserBookingSummary
            {
                BookingId = b.Id,
                MovieTitle = b.Show.Movie.Title,
                PosterUrl = b.Show.Movie.PosterUrl,
                CinemaName = b.Show.CinemaHall.Cinema.Name,
                ShowDate = b.Show.StartTime,
                Amount = b.Amount,
                Status = b.StatusOfPayment.ToString()
            }).ToList();
        }
    }
}