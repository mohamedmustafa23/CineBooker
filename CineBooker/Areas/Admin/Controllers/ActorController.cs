using CineBooker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class ActorController : Controller
    {
        private readonly IActorRepository _actorRepository;

        public ActorController(IActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(FilterActorVM filterActorVM, CancellationToken cancellationToken)
        {
            var actors = await _actorRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);
            if(filterActorVM.name is not null)
            {
                actors = actors.Where(a => a.Name.Contains(filterActorVM.name, StringComparison.OrdinalIgnoreCase));
            }

            if (filterActorVM.gender is not null)
            {
                actors = actors.Where(e => e.Gender.Equals(filterActorVM.gender));
            }

            return View(actors);
        }
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var actors = await _actorRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);
            return View(actors);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Actor actor, IFormFile img, CancellationToken cancellationToken)
        {
            if (img is null)
            {
                ModelState.AddModelError("Error", "Image is required.");
                return View(actor);
            }

            if (img != null && img.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Actors");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var Stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(Stream, cancellationToken);
                }
                actor.PhotoUrl = "/images/Actors/" + fileName;
            }
            await _actorRepository.AddAsync(actor, cancellationToken);
            await _actorRepository.CommitAsync(cancellationToken);
            TempData["Success"] = "Actor created successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var actor = await _actorRepository.GetActorWithMoviesAsync(id, cancellationToken);
            if (actor == null)
            {
                TempData["Error"] = "Actor not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Actor actor, IFormFile? img, CancellationToken cancellationToken)
        {
            var existingActor = await _actorRepository.GetOneAsync(a => a.Id == id, cancellationToken: cancellationToken);

            if (existingActor == null)
            {
                TempData["Error"] = "Actor not found.";
                return RedirectToAction(nameof(Index));
            }
            if (img != null && img.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Actors");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(uploadDir, fileName);
                using (var Stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(Stream, cancellationToken);
                }

                if (!string.IsNullOrEmpty(existingActor.PhotoUrl))
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingActor.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }
                existingActor.PhotoUrl = "/images/actors/" + fileName; 
            }
            
            existingActor.Name = actor.Name;
            existingActor.Bio = actor.Bio;
            existingActor.Gender = actor.Gender;
            existingActor.MovieActors = actor.MovieActors;

            await _actorRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Actor updated successfully."; 
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id, cancellationToken: cancellationToken);
            if (actor == null)
            {
                TempData["Error"] = "Actor not found.";
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrEmpty(actor.PhotoUrl))
            {
                var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", actor.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            _actorRepository.Delete(actor); 
            await _actorRepository.CommitAsync(cancellationToken);
            TempData["Success"] = "Actor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var actor = await _actorRepository.GetOneAsync(
                a => a.Id == id,
                include: query => query
                    .Include(a => a.MovieActors)
                        .ThenInclude(ma => ma.Movie)
            );

            if (actor == null)
            {
                TempData["Error"] = "Actor not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }
    }
}
