using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaAdmin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRole.SUPER_ADMIN_ROLE},{UserRole.ADMIN_ROLE}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string search, string role)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u => u.Email.Contains(search)
                                              || u.FirstName.Contains(search)
                                              || u.LastName.Contains(search));
            }

            var users = await usersQuery.ToListAsync();
            var userList = new List<UserListVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? UserRole.CUSTOMER_ROLE;

                if (!string.IsNullOrEmpty(role) && role != "All" && userRole != role)
                    continue;

                userList.Add(new UserListVM
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = userRole,
                    Gender = user.Gender,
                    PhotoUrl = user.ProfilePicture,
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd
                });
            }

            ViewData["Roles"] = new List<string> { UserRole.SUPER_ADMIN_ROLE, UserRole.ADMIN_ROLE, UserRole.EMPLOYEE_ROLE, UserRole.CUSTOMER_ROLE };
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentRole"] = role;

            return View(userList);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    EmailConfirmed = true,
                    LockoutEnabled = true 
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? UserRole.CUSTOMER_ROLE;

            return View(user);
        }

        // ================= New Action: Block/Unblock =================
        [HttpPost]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, UserRole.SUPER_ADMIN_ROLE))
            {
                TempData["Error"] = "You cannot block a Super Admin!";
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow; 
                TempData["Success"] = "User unlocked successfully.";
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); 
                TempData["Success"] = "User locked successfully.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}