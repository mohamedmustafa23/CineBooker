using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IRepository<Movie> _movieRepo;
        private readonly IRepository<Cinema> _cinemaRepo;
        private readonly IRepository<Show> _showRepo;
        private readonly IRepository<Genre> _genreRepo;

        public HomeController(
            IRepository<Movie> movieRepo,
            IRepository<Cinema> cinemaRepo,
            IRepository<Show> showRepo,
            IRepository<Genre> genreRepo)
        {
            _movieRepo = movieRepo;
            _cinemaRepo = cinemaRepo;
            _showRepo = showRepo;
            _genreRepo = genreRepo;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new HomeVM();

            var cinemas = await _cinemaRepo.GetAsync(include: c => c.Include(x => x.Address));

            model.Cities = cinemas
                .Where(c => c.Address != null && !string.IsNullOrEmpty(c.Address.City))
                .GroupBy(c => c.Address.City) 
                .Select(g => new CityStatsVM
                {
                    Name = g.Key,
                    CinemaCount = g.Count(),
                    ImageUrl = g.FirstOrDefault(c => !string.IsNullOrEmpty(c.Address.PhotoUrl))?.Address.PhotoUrl
                               ?? "https://images.unsplash.com/photo-1480714378408-67cf0d13bc1b?w=400&h=250&fit=crop"
                })
                .OrderByDescending(c => c.CinemaCount)
                .ToList();

            // ================= 2.(Now Showing) =================
            var activeMovies = await _movieRepo.GetAsync(
                m => m.IsActive,
                include: m => m.Include(x => x.Shows).Include(x => x.MovieGenres).ThenInclude(mg => mg.Genre)
            );

            model.NowShowingMovies = activeMovies
                .Where(m => m.Shows.Any(s => s.StartTime >= DateTime.Today)) 
                .Select(m => new MovieCardVM
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterUrl = m.PosterUrl,
                    Duration = m.DurationInMinutes,
                    Rating = (double)(m.AverageRating ?? 0),
                    Genres = string.Join(", ", m.MovieGenres.Select(mg => mg.Genre.Name).Take(2))
                })
                .Take(12) 
                .ToList();

            // ================= 3.(Upcoming) =================
            model.UpcomingMovies = activeMovies
                .Where(m => m.ReleaseDate > DateTime.Today && !m.Shows.Any(s => s.StartTime <= DateTime.Today.AddDays(7)))
                .OrderBy(m => m.ReleaseDate)
                .Select(m => new MovieCardVM
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterUrl = m.PosterUrl,
                    Genres = string.Join(", ", m.MovieGenres.Select(mg => mg.Genre.Name).Take(2)),
                    ReleaseDate = m.ReleaseDate ?? DateTime.Today
                })
                .Take(6)
                .ToList();

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Cinemas(string city)
        {
            var cinemasQuery = await _cinemaRepo.GetAsync(
                include: c => c.Include(x => x.Address).Include(x => x.CinemaHalls)
            );

            var cinemas = cinemasQuery.ToList();

            if (!string.IsNullOrEmpty(city))
            {
                city = city.ToLower();
                cinemas = cinemas.Where(c => c.Address != null && c.Address.City.ToLower() == city).ToList();

                if (cinemas.Any())
                {
                    ViewData["Title"] = $"Cinemas in {cinemas.First().Address.City}";
                }
            }
            else
            {
                ViewData["Title"] = "All Cinemas";
            }

            var model = new CinemaListVM
            {
                CityName = !string.IsNullOrEmpty(city) ? char.ToUpper(city[0]) + city.Substring(1) : "All Locations",
                Cinemas = cinemas.Select(c => new CinemaCardVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address != null ? $"{c.Address.City}" : "No Address",
                    HallCount = c.CinemaHalls?.Count ?? 0,
                    ImageUrl = c.LogoUrl
                }).ToList()
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> MovieList(string searchTerm, int? genreId, string language, DateTime? filterDate, int? cinemaId)
        {
            var query = _movieRepo.GetAsync(
                include: m => m.Include(x => x.MovieGenres).ThenInclude(mg => mg.Genre)
                               .Include(x => x.Shows).ThenInclude(s => s.CinemaHall)
            );

            var movies = (await query).Where(m => m.IsActive).ToList();

            string cinemaName = "All Movies";
            if (cinemaId.HasValue)
            {
                movies = movies.Where(m => m.Shows.Any(s => s.CinemaHall.CinemaId == cinemaId)).ToList();

                var cinema = await _cinemaRepo.GetOneAsync(c => c.Id == cinemaId);
                if (cinema != null) cinemaName = cinema.Name;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                movies = movies.Where(m => m.Title.ToLower().Contains(searchTerm.ToLower())).ToList();
            }

            if (genreId.HasValue)
            {
                movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId)).ToList();
            }

            if (!string.IsNullOrEmpty(language))
            {
                movies = movies.Where(m => m.Language.ToLower() == language.ToLower()).ToList();
            }

            if (filterDate.HasValue)
            {
                movies = movies.Where(m => m.Shows.Any(s => s.StartTime.Date == filterDate.Value.Date)).ToList();
            }

            var genres = await _genreRepo.GetAsync();

            var availableLanguages = (await _movieRepo.GetAsync()).Select(m => m.Language).Distinct().Where(l => !string.IsNullOrEmpty(l)).ToList();

            var model = new MovieListVM
            {
                Movies = movies,
                SearchTerm = searchTerm,
                GenreId = genreId,
                Language = language,
                FilterDate = filterDate,
                CinemaId = cinemaId,
                CinemaName = cinemaName,

                Genres = genres.Select(g => new SelectListItem { Text = g.Name, Value = g.Id.ToString() }),
                Languages = availableLanguages.Select(l => new SelectListItem { Text = l, Value = l })
            };

            return View(model);
        }

    }
}


