using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaAdmin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class SeatController : Controller
    {
        private readonly IRepository<Seat> _seatRepository;
        private readonly IRepository<CinemaHall> _hallRepository;
        private readonly IRepository<Cinema> _cinemaRepository;

        public SeatController(
            IRepository<Seat> seatRepository,
            IRepository<CinemaHall> hallRepository,
            IRepository<Cinema> cinemaRepository)
        {
            _seatRepository = seatRepository;
            _hallRepository = hallRepository;
            _cinemaRepository = cinemaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? cinemaId, int? hallId)
        {
            var model = new SeatVM
            {
                CinemaId = cinemaId,
                HallId = hallId,
                CinemaList = (await _cinemaRepository.GetAsync()).Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            };

            if (cinemaId.HasValue)
            {
                var halls = await _hallRepository.GetAsync(h => h.CinemaId == cinemaId);
                model.HallList = halls.Select(h => new SelectListItem(h.Name, h.Id.ToString()));
            }

            if (hallId.HasValue)
            {
                model.SelectedHall = await _hallRepository.GetOneAsync(h => h.Id == hallId, include: q => q.Include(x => x.Cinema));

                var seats = await _seatRepository.GetAsync(s => s.CinemaHallId == hallId);
                model.Seats = seats.OrderBy(s => s.SeatRow).ThenBy(s => s.SeatCol).ToList();

                model.MaxRows = model.Seats.Any() ? model.Seats.Max(s => s.SeatRow) : 10;
                model.MaxCols = model.Seats.Any() ? model.Seats.Max(s => s.SeatCol) : 15;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetHallsByCinema(int cinemaId)
        {
            var halls = await _hallRepository.GetAsync(h => h.CinemaId == cinemaId);
            return Json(halls.Select(h => new { id = h.Id, name = h.Name }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSingleSeat(int hallId, string rowChar, int col,CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(rowChar) || col < 1) return RedirectToAction(nameof(Index), new { hallId });

            int rowNumber = char.ToUpper(rowChar[0]) - 64;

            var exists = await _seatRepository.GetOneAsync(s => s.CinemaHallId == hallId && s.SeatRow == rowNumber && s.SeatCol == col);
            if (exists == null)
            {
                var seat = new Seat { CinemaHallId = hallId, SeatRow = rowNumber, SeatCol = col };
                await _seatRepository.AddAsync(seat);
                await _seatRepository.CommitAsync(cancellationToken);
                TempData["Success"] = "Seat added.";
            }
            else
            {
                TempData["Error"] = "Seat already exists.";
            }

            var hall = await _hallRepository.GetOneAsync(h => h.Id == hallId);
            return RedirectToAction(nameof(Index), new { cinemaId = hall.CinemaId, hallId = hallId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(int hallId, int rows, int cols, CancellationToken cancellationToken)
        {
            try
            {
                var existingSeats = await _seatRepository.GetAsync(s => s.CinemaHallId == hallId);
                foreach (var seat in existingSeats)
                {
                    _seatRepository.Delete(seat);
                }

                for (int r = 1; r <= rows; r++)
                {
                    for (int c = 1; c <= cols; c++)
                    {
                        await _seatRepository.AddAsync(new Seat
                        {
                            CinemaHallId = hallId,
                            SeatRow = r,
                            SeatCol = c
                        });
                    }
                }
                await _seatRepository.CommitAsync(cancellationToken);

                TempData["Success"] = $"Created {rows * cols} seats successfully.";

                var hall = await _hallRepository.GetOneAsync(h => h.Id == hallId);
                return RedirectToAction(nameof(Index), new { cinemaId = hall.CinemaId, hallId = hallId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating seats: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSeats([FromBody] DeleteSeatsVM seatsVM, CancellationToken cancellationToken)
        {
            if (seatsVM == null || seatsVM.Ids == null || !seatsVM.Ids.Any())
                return Json(new { success = false, message = "No seats selected (List is empty)." });

            try
            {
                foreach (var id in seatsVM.Ids)
                {
                    var seat = await _seatRepository.GetOneAsync(s => s.Id == id);
                    if (seat != null)
                    {
                        _seatRepository.Delete(seat);
                    }
                }

                await _seatRepository.CommitAsync(cancellationToken);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}