using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBooker.Areas.Admin.Controllers
{
    [Area("Admin")] 
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class AddressController : Controller
    {
        private readonly IRepository<Address> _addressRepository;

        public AddressController(IRepository<Address> addressRepository)
        {
            _addressRepository = addressRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);

            return View(addresses);
        }
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);
            return View(addresses);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Address address, IFormFile img, CancellationToken cancellationToken)
        {
            if (img is null)
            {
                ModelState.AddModelError("Error", "Image is required.");
                return View(address);
            }

            if (img != null && img.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\addresses");
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
                address.PhotoUrl = "/images/addresses/" + fileName;
            }
            await _addressRepository.AddAsync(address, cancellationToken);
            await _addressRepository.CommitAsync(cancellationToken);
            TempData["Success"] = "Address created successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetOneAsync(a => a.Id == id, cancellationToken: cancellationToken);
            if (address == null)
            {
                TempData["Error"] = "Address not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Address address, IFormFile? img, CancellationToken cancellationToken)
        {
            var existingAddress = await _addressRepository.GetOneAsync(a => a.Id == id, cancellationToken: cancellationToken);

            if (existingAddress == null)
            {
                TempData["Error"] = "Address not found.";
                return RedirectToAction(nameof(Index));
            }
            if (!ModelState.IsValid)
            {
                return View(address);
            }
            if (img != null && img.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\addresses");
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

                if (!string.IsNullOrEmpty(existingAddress.PhotoUrl))
                {
                    var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingAddress.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }
                existingAddress.PhotoUrl = "/images/addresses/" + fileName;
            }
            existingAddress.City = address.City;

            await _addressRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Address updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetOneAsync(a => a.Id == id, cancellationToken: cancellationToken);
            if (address == null)
            {
                TempData["Error"] = "Address not found.";
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrEmpty(address.PhotoUrl))
            {
                var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", address.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            _addressRepository.Delete(address);
            await _addressRepository.CommitAsync(cancellationToken);
            TempData["Success"] = "Address deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var address = await _addressRepository.GetOneAsync(a => a.Id == id, include: query=> query.Include(e => e.Cinemas));
            if (address == null)
            {
                TempData["Error"] = "Address not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }
    }
}
