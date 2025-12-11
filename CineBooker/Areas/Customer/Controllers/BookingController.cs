using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;


namespace CineBooker.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IRepository<Show> _showRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<ShowSeat> _showSeatRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(IRepository<Show> showRepo, IRepository<Booking> bookingRepo,
                                 IRepository<ShowSeat> showSeatRepo, UserManager<ApplicationUser> userManager)
        {
            _showRepo = showRepo;
            _bookingRepo = bookingRepo;
            _showSeatRepo = showSeatRepo;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Book(int showId)
        {
            await ReleaseExpiredLocks();

            var show = await _showRepo.GetOneAsync(s => s.Id == showId,
                include: s => s.Include(m => m.Movie).Include(c => c.CinemaHall).ThenInclude(cin => cin.Cinema).Include(ss => ss.ShowSeats).ThenInclude(seat => seat.Seat));

            if (show == null) return NotFound();

            var model = new BookTicketVM
            {
                ShowId = show.Id,
                MovieTitle = show.Movie.Title,
                PosterUrl = show.Movie.PosterUrl,
                CinemaName = show.CinemaHall.Cinema.Name,
                HallName = show.CinemaHall.Name,
                ShowDate = show.StartTime,
                Seats = show.ShowSeats.Select(ss => new SeatMapItem
                {
                    ShowSeatId = ss.Id,
                    Row = ss.Seat.SeatRow,
                    Col = ss.Seat.SeatCol,
                    SeatName = $"{(char)(ss.Seat.SeatRow + 64)}{ss.Seat.SeatCol}",
                    Price = ss.Price,
                    Status = ss.Status
                }).OrderBy(s => s.Row).ThenBy(s => s.Col).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LockSeats(BookTicketVM model)
        {
            var userId = _userManager.GetUserId(User);
            if (model.SelectedShowSeatIds == null || !model.SelectedShowSeatIds.Any())
                return Json(new { success = false, message = "Select seats first." });

            var selectedSeats = await _showSeatRepo.GetAsync(ss => model.SelectedShowSeatIds.Contains(ss.Id));

            if (selectedSeats.Any(s => s.Status != SeatStatus.Available))
            {
                return Json(new { success = false, message = "Some seats were just taken. Refreshing..." });
            }

            var booking = new Booking
            {
                UserId = userId,
                ShowId = model.ShowId,
                BookedDate = DateTime.UtcNow,
                StatusOfPayment = PaymentStatus.Pending,
                NumberOfSeats = selectedSeats.Count(),
                Amount = selectedSeats.Sum(s => s.Price)
            };

            var expiry = DateTime.UtcNow.AddMinutes(10);
            foreach (var seat in selectedSeats)
            {
                seat.Status = SeatStatus.Locked;
                seat.LockExpiration = expiry;
                _showSeatRepo.Update(seat);

                booking.BookingSeats.Add(new BookingSeat { ShowSeatId = seat.Id });
            }

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.CommitAsync(default);

            return Json(new { success = true, bookingId = booking.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _bookingRepo.GetOneAsync(b => b.Id == bookingId && b.UserId == userId,
                include: b => b.Include(s => s.Show).ThenInclude(m => m.Movie)
                               .Include(s => s.Show).ThenInclude(c => c.CinemaHall).ThenInclude(ci => ci.Cinema)
                               .Include(bs => bs.BookingSeats).ThenInclude(ss => ss.ShowSeat).ThenInclude(st => st.Seat));

            if (booking == null || booking.StatusOfPayment != PaymentStatus.Pending)
                return RedirectToAction("Index", "Home");

            var firstSeat = booking.BookingSeats.FirstOrDefault()?.ShowSeat;
            if (firstSeat == null || firstSeat.LockExpiration < DateTime.UtcNow)
            {
                await CancelInternal(booking.Id);
                TempData["Error"] = "Session timed out.";
                return RedirectToAction("Book", new { showId = booking.ShowId });
            }

            var model = new PaymentVM
            {
                BookingId = booking.Id,
                MovieTitle = booking.Show.Movie.Title,
                CinemaDetails = $"{booking.Show.CinemaHall.Cinema.Name} - {booking.Show.CinemaHall.Name}",
                ShowDate = booking.Show.StartTime,
                TotalAmount = booking.Amount,
                NumberOfSeats = booking.NumberOfSeats,
                SeatNumbers = string.Join(", ", booking.BookingSeats.Select(s => $"{(char)(s.ShowSeat.Seat.SeatRow + 64)}{s.ShowSeat.Seat.SeatCol}")),
                LockExpiration = firstSeat.LockExpiration.Value
            };

            return View(model);
        }

        public async Task<IActionResult> CreateStripeSession(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _bookingRepo.GetOneAsync(b => b.Id == bookingId && b.UserId == userId,
                include: b => b.Include(s => s.Show).ThenInclude(m => m.Movie));

            if (booking == null) return NotFound();

            var domain = $"{Request.Scheme}://{Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(booking.Amount * 100), // Stripe uses cents
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = booking.Show.Movie.Title,
                            Description = $"Booking for {booking.NumberOfSeats} seats"
                        },
                    },
                    Quantity = 1,
                },
            },
                Mode = "payment",
                SuccessUrl = $"{domain}/Customer/Booking/ConfirmPayment?bookingId={booking.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Customer/Booking/Payment?bookingId={booking.Id}",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> ConfirmPayment(int bookingId, string session_id)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _bookingRepo.GetOneAsync(
                b => b.Id == bookingId && b.UserId == userId,
                include: src => src
                    .Include(b => b.Show).ThenInclude(s => s.Movie)
                    .Include(b => b.Show).ThenInclude(s => s.CinemaHall).ThenInclude(ch => ch.Cinema)
                    .Include(b => b.BookingSeats).ThenInclude(bs => bs.ShowSeat).ThenInclude(ss => ss.Seat)
            );

            if (booking == null) return NotFound();

            var service = new SessionService();
            Session session = service.Get(session_id);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                if (booking.StatusOfPayment != PaymentStatus.Approved)
                {
                    booking.StatusOfPayment = PaymentStatus.Approved;
                    booking.ConfirmationNumber = session.PaymentIntentId ?? Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                    foreach (var bs in booking.BookingSeats)
                    {
                        bs.ShowSeat.Status = SeatStatus.Booked;
                        bs.ShowSeat.LockExpiration = null;
                        _showSeatRepo.Update(bs.ShowSeat);
                    }

                    await _bookingRepo.CommitAsync(default);
                }

                return View("Details", booking);
            }

            return RedirectToAction("Payment", new { bookingId = bookingId });
        }

        private async Task CancelInternal(int bookingId)
        {
            var booking = await _bookingRepo.GetOneAsync(b => b.Id == bookingId, include: b => b.Include(bs => bs.BookingSeats).ThenInclude(ss => ss.ShowSeat));
            if (booking != null)
            {
                booking.StatusOfPayment = PaymentStatus.Rejected;
                foreach (var item in booking.BookingSeats)
                {
                    item.ShowSeat.Status = SeatStatus.Available;
                    item.ShowSeat.LockExpiration = null;
                }
                await _bookingRepo.CommitAsync(default);
            }
        }

        private async Task ReleaseExpiredLocks()
        {
            var expired = await _showSeatRepo.GetAsync(s => s.Status == SeatStatus.Locked && s.LockExpiration < DateTime.UtcNow);
            foreach (var seat in expired)
            {
                seat.Status = SeatStatus.Available;
                seat.LockExpiration = null;
                _showSeatRepo.Update(seat);
            }
            if (expired.Any()) await _showSeatRepo.CommitAsync(default);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _bookingRepo.GetOneAsync(
                b => b.Id == id && b.UserId == userId,

                include: src => src
                    .Include(b => b.Show).ThenInclude(s => s.Movie)
                    .Include(b => b.Show).ThenInclude(s => s.CinemaHall).ThenInclude(c => c.Cinema)
                    .Include(b => b.BookingSeats).ThenInclude(bs => bs.ShowSeat).ThenInclude(ss => ss.Seat)
            );

            if (booking == null) return NotFound();

            return View(booking);
        }
    }
}