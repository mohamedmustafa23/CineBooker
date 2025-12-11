using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class BookingController : Controller
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<ShowSeat> _showSeatRepository;

        public BookingController(
            IRepository<Booking> bookingRepository,
            IRepository<Movie> movieRepository,
            IRepository<ShowSeat> showSeatRepository)
        {
            _bookingRepository = bookingRepository;
            _movieRepository = movieRepository;
            _showSeatRepository = showSeatRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search, int? movieId, DateTime? date, PaymentStatus? status)
        {
            var query = await _bookingRepository.GetAsync(
                include: src => src
                    .Include(b => b.User)
                    .Include(b => b.Show).ThenInclude(s => s.Movie)
                    .Include(b => b.Show).ThenInclude(s => s.CinemaHall)
                    .Include(b => b.BookingSeats).ThenInclude(bs => bs.ShowSeat).ThenInclude(ss => ss.Seat)
            );

            var bookings = query.ToList();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                bookings = bookings.Where(b =>
                    b.Id.ToString().Contains(search) ||
                    (b.User.UserName != null && b.User.UserName.ToLower().Contains(search)) ||
                    (b.User.Email != null && b.User.Email.ToLower().Contains(search))
                ).ToList();
            }

            if (movieId.HasValue)
                bookings = bookings.Where(b => b.Show.MovieId == movieId).ToList();

            if (date.HasValue)
                bookings = bookings.Where(b => b.BookedDate.Date == date.Value.Date).ToList();

            if (status.HasValue)
                bookings = bookings.Where(b => b.StatusOfPayment == status).ToList();

            bookings = bookings.OrderByDescending(b => b.BookedDate).ToList();

            var model = new BookingIndexVM
            {
                Search = search,
                MovieId = movieId,
                Date = date,
                Status = status,
                Bookings = bookings,

                MovieList = (await _movieRepository.GetAsync()).Select(m => new SelectListItem { Text = m.Title, Value = m.Id.ToString() }),

                TotalBookings = bookings.Count,
                TotalRevenue = bookings.Where(b => b.StatusOfPayment == PaymentStatus.Approved).Sum(b => b.Amount),
                PendingCount = bookings.Count(b => b.StatusOfPayment == PaymentStatus.Pending),
                CancelledCount = bookings.Count(b => b.StatusOfPayment == PaymentStatus.Cancelled) 
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _bookingRepository.GetOneAsync(b => b.Id == id, include: x => x.Include(b => b.BookingSeats).ThenInclude(bs => bs.ShowSeat));

            if (booking == null) return NotFound();

            booking.StatusOfPayment = PaymentStatus.Rejected;

            foreach (var bookingSeat in booking.BookingSeats)
            {
                if (bookingSeat.ShowSeat != null)
                {
                    bookingSeat.ShowSeat.Status = SeatStatus.Available;
                }
            }

            await _bookingRepository.CommitAsync(default);
            TempData["Success"] = "Booking cancelled and seats released.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            var booking = await _bookingRepository.GetOneAsync(b => b.Id == id);
            if (booking == null) return NotFound();

            booking.StatusOfPayment = PaymentStatus.Approved;
            await _bookingRepository.CommitAsync(default);

            TempData["Success"] = "Payment approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingRepository.GetOneAsync(
                b => b.Id == id,
                include: src => src
                    .Include(b => b.User) 
                    .Include(b => b.Show).ThenInclude(s => s.Movie) 
                    .Include(b => b.Show).ThenInclude(s => s.CinemaHall).ThenInclude(ch => ch.Cinema).ThenInclude(c => c.Address) 
                    .Include(b => b.BookingSeats).ThenInclude(bs => bs.ShowSeat).ThenInclude(ss => ss.Seat) 
            );

            if (booking == null) return NotFound();

            return View(booking);
        }
    }
}