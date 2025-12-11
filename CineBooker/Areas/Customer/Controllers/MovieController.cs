using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _movieRepo;

        public MovieController(IRepository<Movie> movieRepo)
        {
            _movieRepo = movieRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepo.GetOneAsync(
                m => m.Id == id,
                include: x => x
                    .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                    .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                    .Include(m => m.Shows) 
            );

            if (movie == null) return NotFound();

            var dates = movie.Shows
                .Where(s => s.StartTime >= DateTime.Today)
                .Select(s => s.StartTime.Date)
                .Distinct()
                .OrderBy(d => d)
                .Take(7)
                .ToList();

            if (!dates.Any())
            {
                for (int i = 0; i < 5; i++) dates.Add(DateTime.Today.AddDays(i));
            }

            var model = new MovieDetailsVM
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                PosterUrl = movie.PosterUrl,
                TrailerUrl = movie.TrailerUrl,
                Duration = movie.DurationInMinutes,
                Rating = (double)(movie.AverageRating ?? 0),
                ReleaseDate = movie.ReleaseDate ?? DateTime.MinValue,
                Language = movie.Language,
                Genres = movie.MovieGenres.Select(g => g.Genre.Name).ToList(),
                Director = movie.Director ?? "Unknown",
                Cast = movie.MovieActors.Select(a => a.Actor.Name).Take(4).ToList(),
                AvailableDates = dates,
                SelectedDate = DateTime.Today
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetShowtimes(int movieId, string date)
        {
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                selectedDate = DateTime.Today;

            var movie = await _movieRepo.GetOneAsync(
                m => m.Id == movieId,
                include: m => m
                    .Include(x => x.Shows).ThenInclude(s => s.CinemaHall).ThenInclude(ch => ch.Cinema).ThenInclude(c => c.Address)
                    .Include(x => x.Shows).ThenInclude(s => s.ShowSeats) 
            );

            if (movie == null) return NotFound();

            var showtimes = movie.Shows
                .Where(s => s.StartTime.Date == selectedDate && s.StartTime > DateTime.Now)
                .GroupBy(s => s.CinemaHall.Cinema)
                .Select(g => new CinemaShowtimeVM
                {
                    CinemaName = g.Key.Name,
                    Address = g.Key.Address != null ? $"{g.Key.Address.City}" : "",
                    Shows = g.Select(s => CalculateShowStatus(s)).OrderBy(s => s.StartTime).ToList()
                })
                .OrderBy(c => c.CinemaName)
                .ToList();

            return PartialView("_ShowtimesList", showtimes);
        }

        private ShowtimeVM CalculateShowStatus(Show show)
        {
            var total = show.ShowSeats?.Count ?? 0;
            var booked = show.ShowSeats?.Count(s => s.Status == SeatStatus.Booked) ?? 0;

            string status = "Available";
            string color = "text-success";
            bool isFull = false;

            if (total > 0)
            {
                double percentage = (double)booked / total;
                if (percentage >= 1) { status = "Full"; color = "text-danger"; isFull = true; }
                else if (percentage >= 0.8) { status = "Almost Full"; color = "text-danger"; }
                else if (percentage >= 0.5) { status = "Filling Fast"; color = "text-warning"; }
            }

            return new ShowtimeVM
            {
                ShowId = show.Id,
                StartTime = show.StartTime,
                HallName = show.CinemaHall.Name,
                Status = status,
                StatusColorClass = color,
                IsFull = isFull
            };
        }
    }
}