using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class ShowController : Controller
    {
        private readonly IRepository<Show> _showRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<CinemaHall> _hallRepository;
        private readonly IRepository<ShowSeat> _showSeatRepository;
        private readonly IRepository<Seat> _seatRepository;

        public ShowController(
            IRepository<Show> showRepository,
            IRepository<Movie> movieRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<CinemaHall> hallRepository,
            IRepository<ShowSeat> showSeatRepository,
            IRepository<Seat> seatRepository)
        {
            _showRepository = showRepository;
            _movieRepository = movieRepository;
            _cinemaRepository = cinemaRepository;
            _hallRepository = hallRepository;
            _showSeatRepository = showSeatRepository;
            _seatRepository = seatRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? movieId, int? cinemaId, DateTime? date, CancellationToken cancellationToken)
        {
            var showsQuery = _showRepository.GetAsync(
                include: src => src
                    .Include(s => s.Movie)
                    .Include(s => s.CinemaHall).ThenInclude(h => h.Cinema)
                    .Include(s => s.ShowSeats) 
                    .Include(s => s.Bookings)  
            );

            var shows = (await showsQuery).ToList();

            if (movieId.HasValue) shows = shows.Where(s => s.MovieId == movieId).ToList();
            if (cinemaId.HasValue) shows = shows.Where(s => s.CinemaHall.CinemaId == cinemaId).ToList();
            if (date.HasValue) shows = shows.Where(s => s.StartTime.Date == date.Value.Date).ToList();

            shows = shows.OrderByDescending(s => s.StartTime).ToList();

            ViewBag.Movies = new SelectList(await _movieRepository.GetAsync(), "Id", "Title", movieId);
            ViewBag.Cinemas = new SelectList(await _cinemaRepository.GetAsync(), "Id", "Name", cinemaId);
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

            return View(shows);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ShowVM
            {
                MovieList = (await _movieRepository.GetAsync()).Select(m => new SelectListItem { Text = m.Title, Value = m.Id.ToString() }),
                CinemaList = (await _cinemaRepository.GetAsync()).Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ShowVM model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateLists(model);
                return View(model);
            }

            try
            {
                var movie = await _movieRepository.GetOneAsync(m => m.Id == model.MovieId);
                DateTime startDateTime = model.ShowDate.Date + model.StartTime;
                DateTime endDateTime = startDateTime.AddMinutes(movie.DurationInMinutes);

                var overlap = await _showRepository.GetOneAsync(s =>
                    s.CinemaHallId == model.CinemaHallId &&
                    s.StartTime < endDateTime && s.EndTime > startDateTime
                );

                if (overlap != null)
                {
                    ModelState.AddModelError("", "Time overlap with another show.");
                    await RepopulateLists(model);
                    return View(model);
                }

                var show = new Show
                {
                    MovieId = model.MovieId,
                    CinemaHallId = model.CinemaHallId,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    IsActive = true
                };

                await _showRepository.AddAsync(show, cancellationToken);
                await _showRepository.CommitAsync(cancellationToken);

                var hallSeats = await _seatRepository.GetAsync(s => s.CinemaHallId == model.CinemaHallId);

                foreach (var seat in hallSeats)
                {
                    var showSeat = new ShowSeat
                    {
                        ShowId = show.Id,
                        SeatId = seat.Id,
                        Status = SeatStatus.Available, 
                        Price = model.BasePrice 
                    };
                    await _showSeatRepository.AddAsync(showSeat, cancellationToken);
                }
                await _showSeatRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Show created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                await RepopulateLists(model);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var show = await _showRepository.GetOneAsync(
                s => s.Id == id,
                include: src => src
                    .Include(s => s.Movie)
                    .Include(s => s.CinemaHall).ThenInclude(ch => ch.Cinema)
                    .Include(s => s.ShowSeats) 
                    .Include(s => s.Bookings)
            );

            if (show == null) return NotFound();

            int currentPrice = show.ShowSeats.FirstOrDefault()?.Price ?? 0;

            var model = new ShowEditVM
            {
                Id = show.Id,
                MovieTitle = show.Movie.Title,
                MovieDuration = show.Movie.DurationInMinutes,
                CinemaName = show.CinemaHall.Cinema.Name,
                HallName = show.CinemaHall.Name,
                ShowDate = show.StartTime.Date,
                StartTime = show.StartTime.TimeOfDay,
                BasePrice = currentPrice,

                TotalSeats = show.ShowSeats.Count,
                BookedCount = show.Bookings.Sum(b => b.NumberOfSeats),
                TotalRevenue = show.Bookings.Sum(b => b.Amount)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShowEditVM model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var showInDb = await _showRepository.GetOneAsync(
                    s => s.Id == model.Id,
                    include: src => src.Include(s => s.ShowSeats)
                );

                if (showInDb == null) return NotFound();

                DateTime startDateTime = model.ShowDate.Date + model.StartTime;
                DateTime endDateTime = startDateTime.AddMinutes(model.MovieDuration);

                var overlap = await _showRepository.GetOneAsync(s =>
                    s.Id != model.Id &&
                    s.CinemaHallId == showInDb.CinemaHallId &&
                    s.StartTime < endDateTime && s.EndTime > startDateTime
                );

                if (overlap != null)
                {
                    ModelState.AddModelError("", "Time overlap.");
                    return View(model);
                }

                showInDb.StartTime = startDateTime;
                showInDb.EndTime = endDateTime;

               
                foreach (var seat in showInDb.ShowSeats.Where(ss => ss.Status == SeatStatus.Available))
                {
                    seat.Price = model.BasePrice;
                    _showSeatRepository.Update(seat);
                }

                _showRepository.Update(showInDb);
                await _showRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Show updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var show = await _showRepository.GetOneAsync(s => s.Id == id, include: i => i.Include(s => s.Bookings));

                if (show == null)
                {
                    TempData["Error"] = "Show not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (show.Bookings != null && show.Bookings.Any())
                {
                    TempData["Error"] = "Cannot delete this show because it has active bookings.";
                    return RedirectToAction(nameof(Index));
                }

                var showSeats = await _showSeatRepository.GetAsync(ss => ss.ShowId == id);

                foreach (var seat in showSeats)
                {
                    _showSeatRepository.Delete(seat);
                }

                _showRepository.Delete(show);

                await _showRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Show and its seats deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting show: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task RepopulateLists(ShowVM model)
        {
            model.MovieList = (await _movieRepository.GetAsync()).Select(m => new SelectListItem { Text = m.Title, Value = m.Id.ToString() });
            model.CinemaList = (await _cinemaRepository.GetAsync()).Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            if (model.CinemaId > 0)
            {
                var halls = await _hallRepository.GetAsync(h => h.CinemaId == model.CinemaId);
                model.HallList = halls.Select(h => new SelectListItem { Text = h.Name, Value = h.Id.ToString() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovieDuration(int movieId)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == movieId);
            return Json(new { duration = movie?.DurationInMinutes ?? 0 });
        }

        [HttpGet]
        public async Task<IActionResult> GetHalls(int cinemaId)
        {
            var halls = await _hallRepository.GetAsync(h => h.CinemaId == cinemaId, include: s => s.Include(h => h.Seats));
            return Json(halls.Select(h => new { id = h.Id, name = h.Name, capacity = h.Seats.Count }));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var show = await _showRepository.GetOneAsync(
                s => s.Id == id,
                include: src => src
                    .Include(s => s.Movie)
                    .Include(s => s.CinemaHall).ThenInclude(ch => ch.Cinema)
                    .Include(s => s.ShowSeats.OrderBy(ss => ss.Seat.SeatRow).ThenBy(ss => ss.Seat.SeatCol))
                    .ThenInclude(ss => ss.Seat)
                    .Include(s => s.Bookings).ThenInclude(b => b.User) 
            );

            if (show == null) return NotFound();

            return View(show);
        }
    }
}