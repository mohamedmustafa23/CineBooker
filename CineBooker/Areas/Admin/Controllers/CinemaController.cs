using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaAdmin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]

    public class CinemaController : Controller
    {
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<CinemaHall> _cinemaHallRepository;
        public CinemaController(IRepository<Cinema> cinemaRepository, IRepository<Address> addressRepository, IRepository<CinemaHall> cinemaHallRepository)
        {
            _cinemaRepository = cinemaRepository;
            _addressRepository = addressRepository;
            _cinemaHallRepository = cinemaHallRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var cinemas = await _cinemaRepository.GetAsync(include: e => e .Include(e => e.Address).Include(e => e.CinemaHalls), cancellationToken: cancellationToken, tracked: false);

            return View(cinemas);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var addresses = await _addressRepository.GetAsync();

            var model = new CinemaVM
            {
                Cinema = new Cinema(),
                AddressList = addresses.Select(a => new SelectListItem
                {
                    Text = a.City,
                    Value = a.Id.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CinemaVM model,CancellationToken cancellationToken)
        {

                model.Cinema.AddressId = model.SelectedAddressId;

                if (model.CinemaLogo != null && model.CinemaLogo.Length > 0)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Cinemas");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CinemaLogo.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var Stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CinemaLogo.CopyToAsync(Stream, cancellationToken);
                    }
                    model.Cinema.LogoUrl = "/images/Cinemas/" + fileName;
                }

                await _cinemaRepository.AddAsync(model.Cinema, cancellationToken);
                await _cinemaRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Cinema created successfully.";
                return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(
                expression: c => c.Id == id,
                include: source => source.Include(c => c.CinemaHalls).ThenInclude(h => h.Seats)
            );

            if (cinema == null) return NotFound();

            var addresses = await _addressRepository.GetAsync();

            var model = new CinemaVM
            {
                Cinema = cinema,
                SelectedAddressId = cinema.AddressId,
                AddressList = addresses.Select(a => new SelectListItem
                {
                    Text = a.City,
                    Value = a.Id.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CinemaVM model, CancellationToken cancellationToken)
        {
                var cinemaInDb = await _cinemaRepository.GetOneAsync(c => c.Id == model.Cinema.Id);
                if (cinemaInDb == null) return NotFound();

                cinemaInDb.Name = model.Cinema.Name;
                cinemaInDb.AddressId = model.SelectedAddressId;

                if (model.CinemaLogo != null && model.CinemaLogo.Length > 0)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\Cinemas");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CinemaLogo.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var Stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CinemaLogo.CopyToAsync(Stream, cancellationToken);
                    }

                    if (!string.IsNullOrEmpty(cinemaInDb.LogoUrl))
                    {
                        var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cinemaInDb.LogoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(existingFilePath))
                        {
                            System.IO.File.Delete(existingFilePath);
                        }
                    }
                    cinemaInDb.LogoUrl = "/images/Cinemas/" + fileName;
                }

                _cinemaRepository.Update(cinemaInDb);
                await _cinemaRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Cinema updated successfully.";
                return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(
                expression: c => c.Id == id,
                include: source => source
                    .Include(c => c.Address)
                    .Include(c => c.CinemaHalls).ThenInclude(h => h.Seats)
            );
            if (cinema == null) return NotFound();

            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);

                if (cinema == null)
                {
                    TempData["Error"] = "Cinema not found.";
                    return RedirectToAction(nameof(Index));
                }


                if (!string.IsNullOrEmpty(cinema.LogoUrl))
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cinema.LogoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                _cinemaRepository.Delete(cinema);

                await _cinemaRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Cinema deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot delete this cinema. It may contain active shows or bookings.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
