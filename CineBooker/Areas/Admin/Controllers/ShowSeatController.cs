using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class ShowSeatController : Controller
    {
        private readonly IRepository<ShowSeat> _showSeatRepository;
        private readonly IRepository<Show> _showRepository;

        public ShowSeatController(
            IRepository<ShowSeat> showSeatRepository,
            IRepository<Show> showRepository)
        {
            _showSeatRepository = showSeatRepository;
            _showRepository = showRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? showId)
        {
            var model = new ShowSeatVM
            {
                ShowId = showId
            };

            var shows = await _showRepository.GetAsync(
                include: s => s.Include(x => x.Movie).Include(x => x.CinemaHall)
            );

            model.ShowList = shows.Select(s => new SelectListItem
            {
                Text = $"{s.Movie.Title} - {s.StartTime:MMM dd, hh:mm tt} ({s.CinemaHall.Name})",
                Value = s.Id.ToString()
            });

            if (showId.HasValue)
            {
                model.SelectedShow = await _showRepository.GetOneAsync(
                    s => s.Id == showId,
                    include: src => src.Include(s => s.Movie).Include(s => s.CinemaHall).ThenInclude(ch => ch.Cinema)
                );

                var seatsQuery = await _showSeatRepository.GetAsync(
                    expression: ss => ss.ShowId == showId,
                    include: src => src.Include(ss => ss.Seat)
                );

                model.ShowSeats = seatsQuery.OrderBy(ss => ss.Seat.SeatRow).ThenBy(ss => ss.Seat.SeatCol).ToList();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSeatLock(int showSeatId)
        {
            var seat = await _showSeatRepository.GetOneAsync(s => s.Id == showSeatId);

            if (seat == null) return Json(new { success = false, message = "Seat not found" });

            if (seat.Status == SeatStatus.Booked)
            {
                return Json(new { success = false, message = "Cannot modify a Booked seat." });
            }

            if (seat.Status == SeatStatus.Locked)
            {
                seat.Status = SeatStatus.Available;
                seat.LockExpiration = null;
            }
            else
            {
                seat.Status = SeatStatus.Locked;
                seat.LockExpiration = DateTime.UtcNow.AddMinutes(10); 
            }

            await _showSeatRepository.CommitAsync(default);

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleaseAllLocks(int showId)
        {
            var lockedSeats = await _showSeatRepository.GetAsync(
                s => s.ShowId == showId && s.Status == SeatStatus.Locked
            );

            foreach (var seat in lockedSeats)
            {
                seat.Status = SeatStatus.Available;
                seat.LockExpiration = null;
            }

            await _showSeatRepository.CommitAsync(default);
            return Json(new { success = true });
        }
    }
}