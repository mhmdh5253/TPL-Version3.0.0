using BE;
using BE.LetterAutomation;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace TPLWeb.Controllers
{

    [Authorize(Roles = "Admin")]
    public class UserOrganizationController : Controller
    {
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserOrganizationController(
            Db context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserOrganization
        [HttpGet]
        public async Task<IActionResult> Index(int? organizationId)
        {
            var query = _context.UserOrganizations
                .Include(uo => uo.User)
                .Include(uo => uo.Organization)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                query = query.Where(uo => uo.OrganizationId == organizationId.Value);
            }

            var model = await query.Where(x=>x.IsActive).ToListAsync();
            ViewBag.Organizations = await _context.Organizations.ToListAsync();
            return View(model);
        }

        // GET: UserOrganization/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();
            return View();
        }

        // POST: UserOrganization/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserOrganization userOrganization)
        {
            if (ModelState.IsValid)
            {
                // بررسی تکراری نبودن رابطه کاربر و سازمان
                var user = await _userManager.FindByIdAsync(userOrganization.UserId!);
                if (user == null)
                {
                    ModelState.AddModelError("", "کاربر مورد نظر یافت نشد.");
                }
                else if (user.OrganizationId != null && user.OrganizationId != userOrganization.OrganizationId)
                {
                    ModelState.AddModelError("", "این کاربر قبلاً به یک سازمان دیگر اختصاص داده شده است.");
                }
                else
                {
                    // به‌روزرسانی سازمان کاربر
                    user.OrganizationId = userOrganization.OrganizationId;
                    
                    // Update user's organization-related profile fields
                    var organization = await _context.Organizations.FindAsync(userOrganization.OrganizationId);
                    if (organization != null)
                    {
                        user.CompanyName = organization.Name;
                        user.NameEdareh = organization.Name;
                        user.Semat = userOrganization.Position;
                    }
                    
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        userOrganization.StartDate = DateTime.Now;
                        userOrganization.IsActive = true;
                        _context.Add(userOrganization);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            await PrepareDropdowns(userOrganization.UserId);
            return View(userOrganization);
        }
        // GET: UserOrganization/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userOrganization = await _context.UserOrganizations
                .Include(uo => uo.User)
                .Include(uo => uo.Organization)
                .FirstOrDefaultAsync(uo => uo.Id == id);

            if (userOrganization == null)
            {
                return NotFound();
            }

            await PrepareDropdowns(userOrganization.UserId);
            return View("CreateOrEdit", userOrganization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserOrganization userOrganization)
        {
            if (id != userOrganization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUserOrg = await _context.UserOrganizations.FindAsync(id);
                    if (existingUserOrg == null)
                    {
                        return NotFound();
                    }

                    // به‌روزرسانی سازمان کاربر
                    var user = await _userManager.FindByIdAsync(existingUserOrg.UserId!);
                    if (user != null)
                    {
                        // بررسی اینکه آیا سازمان جدید قبلاً به کاربر دیگری اختصاص داده شده است
                        var orgAlreadyAssigned = await _context.Users
                            .AnyAsync(u => u.Id != user.Id && u.OrganizationId == userOrganization.OrganizationId);

                        if (orgAlreadyAssigned)
                        {
                            ModelState.AddModelError("OrganizationId", "این سازمان قبلاً به کاربر دیگری اختصاص داده شده است.");
                            await PrepareDropdowns(userOrganization.UserId);
                            return View("CreateOrEdit", userOrganization);
                        }

                        user.OrganizationId = userOrganization.OrganizationId;
                        
                        // Update user's organization-related profile fields
                        var organization = await _context.Organizations.FindAsync(userOrganization.OrganizationId);
                        if (organization != null)
                        {
                            user.CompanyName = organization.Name;
                            user.NameEdareh = organization.Name;
                            user.Semat = userOrganization.Position;
                        }
                        
                        await _userManager.UpdateAsync(user);
                    }

                    existingUserOrg.OrganizationId = userOrganization.OrganizationId;
                    existingUserOrg.StartDate = userOrganization.StartDate;
                    existingUserOrg.EndDate = userOrganization.EndDate;
                    existingUserOrg.IsActive = userOrganization.IsActive;

                    _context.Update(existingUserOrg);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserOrganizationExists(userOrganization.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PrepareDropdowns(userOrganization.UserId);
            return View("CreateOrEdit", userOrganization);
        }

        // GET: UserOrganization/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userOrganization = await _context.UserOrganizations
                .Include(uo => uo.User)
                .Include(uo => uo.Organization)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (userOrganization == null)
            {
                return NotFound();
            }

            return View(userOrganization);
        }

        // GET: UserOrganization/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userOrganization = await _context.UserOrganizations
                .Include(uo => uo.User)
                .Include(uo => uo.Organization)
                .FirstOrDefaultAsync(uo => uo.Id == id);

            if (userOrganization == null)
            {
                return NotFound();
            }

            return View(userOrganization);
        }

        // در متد DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userOrganization = await _context.UserOrganizations.FindAsync(id);
            if (userOrganization == null)
            {
                return NotFound();
            }

            // حذف سازمان از کاربر
            var user = await _userManager.FindByIdAsync(userOrganization.UserId!);
            if (user != null)
            {
                user.OrganizationId = null;
                // Clear organization-related profile fields
                user.CompanyName = "";
                user.NameEdareh = "";
                user.Semat = "";
                await _userManager.UpdateAsync(user);
            }

            userOrganization.IsActive = false;
            userOrganization.EndDate = DateTime.Now;

            _context.Update(userOrganization);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: تغییر وضعیت فعال/غیرفعال
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var userOrganization = await _context.UserOrganizations
                    .Include(uo => uo.User)
                    .Include(uo => uo.Organization)
                    .FirstOrDefaultAsync(uo => uo.Id == id);
                    
                if (userOrganization == null)
                {
                    return Json(new { success = false, message = "رابطه کاربر و سازمان یافت نشد" });
                }

                // تغییر وضعیت
                var oldStatus = userOrganization.IsActive;
                userOrganization.IsActive = !userOrganization.IsActive;
                
                if (userOrganization.IsActive)
                {
                    // اگر فعال شد، تاریخ پایان را پاک کن
                    userOrganization.EndDate = null;
                    
                    // بررسی اینکه آیا سازمان قبلاً به کاربر دیگری اختصاص داده شده است
                    var orgAlreadyAssigned = await _context.Users
                        .AnyAsync(u => u.Id != userOrganization.UserId && u.OrganizationId == userOrganization.OrganizationId);
                        
                    if (orgAlreadyAssigned)
                    {
                        return Json(new { success = false, message = "این سازمان قبلاً به کاربر دیگری اختصاص داده شده است" });
                    }
                    
                    // به‌روزرسانی فیلدهای سازمانی کاربر
                    if (userOrganization.User != null && userOrganization.Organization != null)
                    {
                        userOrganization.User.CompanyName = userOrganization.Organization.Name;
                        userOrganization.User.NameEdareh = userOrganization.Organization.Name;
                        userOrganization.User.Semat = userOrganization.Position;
                        userOrganization.User.OrganizationId = userOrganization.OrganizationId;
                        await _userManager.UpdateAsync(userOrganization.User);
                    }
                }
                else
                {
                    // اگر غیرفعال شد، تاریخ پایان را تنظیم کن
                    userOrganization.EndDate = DateTime.Now;
                    
                    // پاک کردن فیلدهای سازمانی کاربر
                    if (userOrganization.User != null)
                    {
                        userOrganization.User.CompanyName = "";
                        userOrganization.User.NameEdareh = "";
                        userOrganization.User.Semat = "";
                        userOrganization.User.OrganizationId = null;
                        await _userManager.UpdateAsync(userOrganization.User);
                    }
                }

                _context.Update(userOrganization);
                await _context.SaveChangesAsync();

                var statusText = userOrganization.IsActive ? "فعال" : "غیرفعال";
                var message = $"وضعیت کاربر '{userOrganization.User?.FirstName} {userOrganization.User?.LastName}' در سازمان '{userOrganization.Organization?.Name}' به {statusText} تغییر یافت";

                return Json(new { 
                    success = true, 
                    isActive = userOrganization.IsActive,
                    message = message,
                    oldStatus = oldStatus,
                    newStatus = userOrganization.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"خطا در تغییر وضعیت: {ex.Message}" 
                });
            }
        }




        private bool UserOrganizationExists(int id)
        {
            return _context.UserOrganizations.Any(e => e.Id == id);
        }

        private async Task PrepareDropdowns(string? currentUserId = null)
        {
            // لیست کاربران بدون سازمان یا کاربر فعلی
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                usersQuery = usersQuery
                    .Where(u => u.Id == currentUserId || u.OrganizationId == null);
            }
            else
            {
                usersQuery = usersQuery.Where(u => u.OrganizationId == null);
            }

            ViewBag.Users = await usersQuery
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName} ({u.UserName})",
                    Selected = u.Id == currentUserId
                })
                .ToListAsync();

            // لیست سازمان‌های بدون کاربر یا سازمان مربوط به کاربر فعلی
            var organizationsQuery = _context.Organizations.AsQueryable();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var currentUserOrgId = await _userManager.Users
                    .Where(u => u.Id == currentUserId)
                    .Select(u => u.OrganizationId)
                    .FirstOrDefaultAsync();

                organizationsQuery = organizationsQuery
                    .Where(o => !_context.Users.Any(u => u.OrganizationId == o.Id && u.Id != currentUserId) ||
                                o.Id == currentUserOrgId);
            }
            else
            {
                organizationsQuery = organizationsQuery
                    .Where(o => !_context.Users.Any(u => u.OrganizationId == o.Id));
            }

            ViewBag.Organizations = await organizationsQuery
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Name
                })
                .ToListAsync();
        }
    }
}