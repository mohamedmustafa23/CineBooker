using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaAdmin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class CinemaHallController : Controller
    {
        private readonly IRepository<CinemaHall> _cinemaHallRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Seat> _seatRepository;

        public CinemaHallController(IRepository<CinemaHall> cinemaHallRepository, IRepository<Cinema> cinemaRepository, IRepository<Seat> seatRepository)
        {
            _cinemaHallRepository = cinemaHallRepository;
            _cinemaRepository = cinemaRepository;
            _seatRepository = seatRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? cinemaId, string search, CancellationToken cancellationToken)
        {
            var hallsQuery = await _cinemaHallRepository.GetAsync(
                include: source => source
                    .Include(h => h.Cinema)
                    .Include(h => h.Seats),
                cancellationToken: cancellationToken,
                tracked: false
            );

            var halls = hallsQuery.ToList();

            if (cinemaId.HasValue && cinemaId.Value > 0)
            {
                halls = halls.Where(h => h.CinemaId == cinemaId).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                halls = halls.Where(h => h.Name.ToLower().Contains(search.ToLower())).ToList();
            }

            var cinemas = await _cinemaRepository.GetAsync(cancellationToken: cancellationToken);
            ViewBag.CinemasList = new SelectList(cinemas, "Id", "Name", cinemaId);

            return View(halls);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var hall = await _cinemaHallRepository.GetOneAsync(h => h.Id == id);
                if (hall == null)
                {
                    TempData["Error"] = "Hall not found.";
                    return RedirectToAction(nameof(Index));
                }

                _cinemaHallRepository.Delete(hall);
                await _cinemaHallRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Cinema Hall deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot delete hall. It may contain seats or active shows.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var cinemas = await _cinemaRepository.GetAsync();

            var model = new CinemaHallVM
            {
                CinemaList = cinemas.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CinemaHallVM model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var cinemas = await _cinemaRepository.GetAsync();
                model.CinemaList = cinemas.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(model);
            }

            try
            {
                var hall = new CinemaHall
                {
                    Name = model.Name,
                    CinemaId = model.CinemaId
                };

                await _cinemaHallRepository.AddAsync(hall, cancellationToken);
                await _cinemaHallRepository.CommitAsync(cancellationToken); 

                for (int row = 1; row <= model.Rows; row++)
                {
                    for (int col = 1; col <= model.Columns; col++)
                    {
                        var seat = new Seat
                        {
                            SeatRow = row,
                            SeatCol = col,
                            CinemaHallId = hall.Id,
                        };
                        await _seatRepository.AddAsync(seat, cancellationToken);
                    }
                }

                await _seatRepository.CommitAsync(cancellationToken);

                TempData["Success"] = $"Hall created with {model.Rows * model.Columns} seats generated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);

                var cinemas = await _cinemaRepository.GetAsync();
                model.CinemaList = cinemas.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hall = await _cinemaHallRepository.GetOneAsync(
                expression: h => h.Id == id,
                include: source => source.Include(h => h.Seats)
            );

            if (hall == null) return NotFound();

            int maxRows = hall.Seats?.Any() == true ? hall.Seats.Max(s => s.SeatRow) : 0;
            int maxCols = hall.Seats?.Any() == true ? hall.Seats.Max(s => s.SeatCol) : 0;

            var cinemas = await _cinemaRepository.GetAsync();

            var model = new CinemaHallVM
            {
                Id = hall.Id,
                Name = hall.Name,
                CinemaId = hall.CinemaId,
                Rows = maxRows,     
                Columns = maxCols, 
                CinemaList = cinemas.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CinemaHallVM model, CancellationToken cancellationToken)
        {

            ModelState.Remove("Rows");
            ModelState.Remove("Columns");

            if (!ModelState.IsValid)
            {
                var cinemas = await _cinemaRepository.GetAsync();
                model.CinemaList = cinemas.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(model);
            }

            try
            {
                var hallInDb = await _cinemaHallRepository.GetOneAsync(h => h.Id == model.Id);
                if (hallInDb == null) return NotFound();


                hallInDb.Name = model.Name;
                hallInDb.CinemaId = model.CinemaId;


                _cinemaHallRepository.Update(hallInDb);
                await _cinemaHallRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Cinema Hall updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                var cinemas = await _cinemaRepository.GetAsync();
                model.CinemaList = cinemas.Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var hall = await _cinemaHallRepository.GetOneAsync(
                expression: h => h.Id == id,
                include: source => source
                    .Include(h => h.Cinema)
                    .Include(h => h.Seats)
            );

            if (hall == null) return NotFound();

            return View(hall);
        }
    }
}