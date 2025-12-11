using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAdmin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class GenreController : Controller
    {
        private readonly IGenreRepository _genreRepository;
        public GenreController(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var genres = await _genreRepository.GetAllWithMoviesAsync(cancellationToken: cancellationToken);
            return View(genres);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Genre genre, CancellationToken cancellationToken)
        {
            await _genreRepository.AddAsync(genre);
            await _genreRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _genreRepository.GetOneAsync(g => g.Id == id);
            return View(genre);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Genre genre, CancellationToken cancellationToken)
        {
            _genreRepository.Update(genre);
            await _genreRepository.CommitAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }
}
