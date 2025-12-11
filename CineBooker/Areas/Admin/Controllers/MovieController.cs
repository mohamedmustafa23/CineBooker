using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaAdmin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IActorRepository _actorRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IRepository<MovieActor> _movieActorRepository;
        private readonly IRepository<MovieGenre> _movieGenreRepository;

        public MovieController(
            IRepository<Movie> movieRepository,
            IActorRepository actorRepository,
            IGenreRepository genreRepository,
            IRepository<MovieActor> movieActorRepository,
            IRepository<MovieGenre> movieGenreRepository)
        {
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _genreRepository = genreRepository;
            _movieActorRepository = movieActorRepository;
            _movieGenreRepository = movieGenreRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetAsync(include: query => query.Include(e => e.MovieGenres).ThenInclude(m => m.Genre), cancellationToken: cancellationToken, tracked: false);
            return View(movies);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var genres = await _genreRepository.GetAsync();
            var actors = await _actorRepository.GetAsync();

            return View(new MovieVM
            {
                Genres = genres.ToList(),
                Actors = actors.ToList(),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Movie movie,
            IFormFile PosterFile,
            List<int> GenreIds,
            List<MovieActorVM> MovieActors,
            CancellationToken cancellationToken)
        {
            try
            {
                if (PosterFile == null || PosterFile.Length == 0)
                {
                    ModelState.AddModelError("PosterFile", "Poster image is required.");
                    return View(movie);
                }

                if (GenreIds == null || !GenreIds.Any())
                {
                    ModelState.AddModelError("GenreIds", "Please select at least one genre.");
                    return View(movie);
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Movies");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PosterFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PosterFile.CopyToAsync(stream, cancellationToken);
                }

                movie.PosterUrl = "/images/Movies/" + fileName;
                movie.CreatedAt = DateTime.UtcNow;

                await _movieRepository.AddAsync(movie, cancellationToken);
                await _movieRepository.CommitAsync(cancellationToken);

                foreach (var genreId in GenreIds)
                {
                    var movieGenre = new MovieGenre
                    {
                        MovieId = movie.Id,
                        GenreId = genreId
                    };
                    await _movieGenreRepository.AddAsync(movieGenre, cancellationToken);
                }

                if (MovieActors != null && MovieActors.Any())
                {
                    foreach (var actor in MovieActors)
                    {
                        var movieActor = new MovieActor
                        {
                            MovieId = movie.Id,
                            ActorId = actor.ActorId,
                            CharacterName = actor.CharacterName
                        };
                        await _movieActorRepository.AddAsync(movieActor, cancellationToken);
                    }
                }

                await _movieRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Movie created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the movie: " + ex.Message);
                return View(movie);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepository.GetAsync(
                 m => m.Id == id,
                include: q => q.Include(m => m.MovieGenres)
                               .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            );

            if (movie == null || !movie.Any())
            {
                TempData["Error"] = "Movie not found.";
                return RedirectToAction(nameof(Index));
            }

            var genres = await _genreRepository.GetAsync();
            var actors = await _actorRepository.GetAsync();

            var movieVM = new MovieVM
            {
                movie = movie.First(),
                Genres = genres.ToList(),
                Actors = actors.ToList()
            };

            return View(movieVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Movie movie,
            IFormFile PosterFile,
            List<int> GenreIds,
            List<MovieActorVM> MovieActors,
            CancellationToken cancellationToken)
        {
            if (id != movie.Id)
            {
                TempData["Error"] = "Invalid movie ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existingMovie = await _movieRepository.GetAsync(
                    expression: m => m.Id == id,
                    include: q => q.Include(m => m.MovieGenres)
                                   .Include(m => m.MovieActors)
                );

                if (existingMovie == null || !existingMovie.Any())
                {
                    TempData["Error"] = "Movie not found.";
                    return RedirectToAction(nameof(Index));
                }

                var movieToUpdate = existingMovie.First();

                if (PosterFile != null && PosterFile.Length > 0)
                {
                    if (PosterFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("PosterFile", "Image size must not exceed 5MB.");
                        return await ReloadEditView(movie);
                    }

                    if (!string.IsNullOrEmpty(movieToUpdate.PosterUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", movieToUpdate.PosterUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Movies");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PosterFile.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PosterFile.CopyToAsync(stream, cancellationToken);
                    }

                    movieToUpdate.PosterUrl = "/images/Movies/" + fileName;
                }

                movieToUpdate.Title = movie.Title;
                movieToUpdate.Description = movie.Description;
                movieToUpdate.DurationInMinutes = movie.DurationInMinutes;
                movieToUpdate.Director = movie.Director;
                movieToUpdate.Language = movie.Language;
                movieToUpdate.ReleaseDate = movie.ReleaseDate;
                movieToUpdate.Rating = movie.Rating;
                movieToUpdate.TrailerUrl = movie.TrailerUrl;
                movieToUpdate.IsActive = movie.IsActive;

                
                foreach (var oldGenre in movieToUpdate.MovieGenres.ToList())
                {
                    _movieGenreRepository.Delete(oldGenre);
                }

                if (GenreIds != null && GenreIds.Any())
                {
                    foreach (var genreId in GenreIds)
                    {
                        var movieGenre = new MovieGenre
                        {
                            MovieId = movieToUpdate.Id,
                            GenreId = genreId
                        };
                        await _movieGenreRepository.AddAsync(movieGenre, cancellationToken);
                    }
                }

                
                foreach (var oldActor in movieToUpdate.MovieActors.ToList())
                {
                    _movieActorRepository.Delete(oldActor);
                }

               
                if (MovieActors != null && MovieActors.Any())
                {
                    foreach (var actor in MovieActors)
                    {
                        var movieActor = new MovieActor
                        {
                            MovieId = movieToUpdate.Id,
                            ActorId = actor.ActorId,
                            CharacterName = actor.CharacterName
                        };
                        await _movieActorRepository.AddAsync(movieActor, cancellationToken);
                    }
                }

                await _movieRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Movie updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the movie: " + ex.Message);
                return View(movie);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var movie = await _movieRepository.GetAsync(
                    expression: m => m.Id == id,
                    include: q => q.Include(m => m.MovieGenres)
                                   .Include(m => m.MovieActors)
                );

                if (movie == null || !movie.Any())
                {
                    TempData["Error"] = "Movie not found.";
                    return RedirectToAction(nameof(Index));
                }

                var movieToDelete = movie.First();

                if (!string.IsNullOrEmpty(movieToDelete.PosterUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", movieToDelete.PosterUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _movieRepository.Delete(movieToDelete);
                await _movieRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Movie deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the movie: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepository.GetAsync(
                expression: m => m.Id == id,
                include: q => q.Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                               .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            );

            if (movie == null || !movie.Any())
            {
                TempData["Error"] = "Movie not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(movie.First());
        }


        private async Task<IActionResult> ReloadEditView(Movie movie)
        {
            var genres = await _genreRepository.GetAsync();
            var actors = await _actorRepository.GetAsync();

            return View(new MovieVM
            {
                movie = movie,
                Genres = genres.ToList(),
                Actors = actors.ToList()
            });
        }

    }
}