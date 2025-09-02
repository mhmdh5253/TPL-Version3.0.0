using BE;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ManageRoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageRoleController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            TempData["firstload"] = true;
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string name)
        {
            var role = new ApplicationRole { Name = name };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deletion of SuperAdmin, Admin, and User roles
            if (role.Name == "SuperAdmin" || role.Name == "Admin" || role.Name == "User")
            {
                TempData["ErrorMessage"] = "نقش های پیش فررض قابل حذف نیستند";
                return RedirectToAction("Index");
            }

            // Find users in the role being deleted
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

            // Assign users to a higher-level role (e.g., "Admin")
            var higherRole = await _roleManager.FindByNameAsync("Admin"); // You can adjust the target role as needed
            if (higherRole != null)
            {
                foreach (var user in usersInRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name!);
                    await _userManager.AddToRoleAsync(user, higherRole.Name!);
                }
            }
            else
            {
                // Handle the case where the higher-level role doesn't exist
                TempData["ErrorMessage"] = "Higher-level role 'Admin' not found.  Cannot reassign users.";
                return RedirectToAction("Index");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index"); // Or return to a suitable error view
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string id, string name)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(role);
        }
        [HttpGet]
        public async Task<IActionResult> GetUsersInRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound();

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);

            return Ok(users.Select(u => new {
                id = u.Id,
                userName = u.UserName,
                email = u.Email,
                fullName = u.FirstName+"-"+u.LastName,
                roleId = role.Id,
                avatar = string.IsNullOrWhiteSpace(u.Avatar) ? null : Url.Content($"~/CompanyVariables/avatars/{u.Avatar}")
            }));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (user?.UserName=="mhmdh5253")
            {
                return NotFound();

            }
            if (user == null || role == null)
                return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);

            if (result.Succeeded)
                return Ok();

            return BadRequest();
        }
    }
}