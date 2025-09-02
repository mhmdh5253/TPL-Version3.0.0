using BE.LetterAutomation;
using BLL.LetterAutomation;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TPLWeb.Controllers
{
    [Route("Organization")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OrganizationController : Controller
    {
        private readonly Db _context;
        private readonly BlRecivers _recivers;
        private readonly BLL.Ticketing.BlNotification _notificationService;
        public OrganizationController(Db context, BlRecivers recivers, BLL.Ticketing.BlNotification notificationService)
        {
            _context = context;
            _recivers = recivers;
            _notificationService = notificationService;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // دریافت تمام سطوح سازمانی به صورت بازگشتی
                var mainOrganizations = await _context.Organizations
                    .Where(o => o.IsMain && o.IsActive)
                    .Include(o => o.Children.Where(c => c.IsActive))
                    .ThenInclude(c => c.Children.Where(cc => cc.IsActive))
                    .ThenInclude(cc => cc.Children.Where(ccc => ccc.IsActive))
                    .OrderBy(o => o.Name)
                    .AsNoTracking()
                    .ToListAsync();

                var model = new OrganizationViewModel
                {
                    MainOrganizations = mainOrganizations.Select(o => MapToTreeItem(o)).ToList(),
                    FormModel = new OrganizationFormModel()
                };

                return View(model);
            }
            catch
            {
                return StatusCode(500, "خطا در پردازش درخواست");
            }
        }

        // تابع کمکی برای تبدیل Organization به OrganizationTreeItem به صورت بازگشتی
        private OrganizationTreeItem MapToTreeItem(Organization org)
        {
            var treeItem = new OrganizationTreeItem
            {
                Id = org.Id,
                Name = org.Name,
                OrgType = org.OrgType,
                SubOrganizations = new List<OrganizationTreeItem>()
            };

            if (org.Children != null && org.Children.Any())
            {
                foreach (var child in org.Children.OrderBy(c => c.Name))
                {
                    treeItem.SubOrganizations.Add(MapToTreeItem(child));
                }
            }

            return treeItem;
        }
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            return View("CreateEdit", new OrganizationFormModel());
        }

        // POST: ایجاد سازمان جدید
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationFormModel formModel)
        {
            if (ModelState.IsValid)
            {
                var recivers = await _recivers.GetAllOrganizationsChainingAsync();
                var organization = new Organization
                {
                    Name = formModel.Name,
                    OrgType = formModel.OrgType,
                    ParentId = formModel.ParentId,
                    Code = formModel.Code,
                    Description = formModel?.Description! ?? " ",
                    IsMain = formModel?.ParentId == null,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    Address = formModel?.Address! ?? " ",
                    Email = formModel?.Email! ?? " ",
                    Phone = formModel?.Phone! ?? " ",
                    FullName = _recivers.GetReceiverFullPath(formModel!.ParentId.ToString()!) + "-" + formModel!.Name,
                };


                _context.Add(organization);
                await _context.SaveChangesAsync();
                // notify admins
                await NotifyAdmins($"سازمان جدید '{organization.Name}' ایجاد شد");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            return View("CreateEdit", formModel);
        }

        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            var formModel = new OrganizationFormModel
            {
                Id = organization.Id,
                Name = organization.Name,
                OrgType = organization.OrgType,
                ParentId = organization.ParentId,
                Code = organization.Code,
                Description = organization.Description,
                Address = organization.Address,
                Email = organization.Email,
                Phone = organization.Phone
            };

            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive && o.Id != id)
                .ToList();
            return View("CreateEdit", formModel);
        }

        // POST: ویرایش سازمان
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrganizationFormModel formModel)
        {
            if (id != formModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return NotFound();
                }

                organization.Name = formModel.Name;
                organization.OrgType = formModel.OrgType;
                organization.ParentId = formModel.ParentId;
                organization.Code = formModel.Code;
                organization.Description = formModel?.Description! ?? " ";
                organization.IsMain = formModel!.ParentId == null;
                organization.UpdatedAt = DateTime.Now;
                organization.Address = formModel?.Address!;
                organization.Email = formModel?.Email!;
                organization.Phone = formModel?.Phone!;
                try
                {
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                    await NotifyAdmins($"سازمان '{organization.Name}' به‌روزرسانی شد");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive && o.Id != id)
                .ToList();
            return View("CreateEdit", formModel);
        }

        // POST: تغییر وضعیت فعال/غیرفعال سازمان
        [HttpPost("ToggleStatus")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            organization.IsActive = !organization.IsActive;
            organization.UpdatedAt = DateTime.Now;

            _context.Update(organization);
            await _context.SaveChangesAsync();
            await NotifyAdmins($"وضعیت سازمان '{organization.Name}' به {(organization.IsActive ? "فعال" : "غیرفعال")} تغییر یافت");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var organization = await _context.Organizations
                    .Include(o => o.Children)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (organization == null)
                {
                    TempData["ErrorMessage"] = "سازمان مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // حذف نرم تمام زیرمجموعه‌ها
                await SoftDeleteOrganizationRecursive(organization);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"سازمان {organization.Name} با موفقیت حذف شد";
                await NotifyAdmins($"سازمان '{organization.Name}' حذف شد");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "خطا در حذف سازمان";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task SoftDeleteOrganizationRecursive(Organization org)
        {
            foreach (var child in org.Children.ToList())
            {
                await SoftDeleteOrganizationRecursive(child);
            }
            org.IsActive = false;
            org.UpdatedAt = DateTime.Now;
        }
        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id);
        }

        [HttpGet("SearchOrganizations")]
        public async Task<IActionResult> SearchOrganizations(string term)
        {
            var organizations = await _context.Organizations
                .Where(o => o.IsActive && o.Name!.Contains(term))
                .Select(o => new { id = o.Id, text = o.Name })
                .ToListAsync();

            return Json(organizations);
        }


        // GET: نمایش جزئیات یک سازمان
        [HttpGet("Details")]
        public async Task<IActionResult> Details(int id)
        {
            var organization = await _context.Organizations
                .AsNoTracking() // بهبود عملکرد برای عملیات خواندن
                .Include(o => o.Parent)
                .ThenInclude(p => p!.Parent) // لود کردن سطوح بالاتر سلسله مراتب
                .Include(o => o.Children)
                .ThenInclude(c => c.Children.Where(x => x.IsActive)) // فیلتر خودکار زیرمجموعه‌های فعال
                .ThenInclude(cc => cc.Children) // لود کردن سطح سوم
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive); // فیلتر خودکار سازمان‌های فعال

            if (organization == null)
            {
                return NotFound();
            }

            // محاسبه مسیر سلسله مراتبی
            var path = new List<string>();
            var currentOrg = organization;
            while (currentOrg != null)
            {
                path.Add(currentOrg.Name!);
                currentOrg = currentOrg.Parent;
            }
            path.Reverse();
            ViewBag.FullPath = string.Join(" > ", path);

            // محاسبه اطلاعات آماری
            ViewBag.Stats = new
            {
                TotalChildren = await CountChildrenRecursive(organization.Id),
                ActiveChildren = await CountChildrenRecursive(organization.Id, true)
            };

            return PartialView("_OrganizationDetails", organization);
        }


        private async Task<int> CountChildrenRecursive(int parentId, bool onlyActive = false)
        {
            var query = _context.Organizations
                .Where(o => o.ParentId == parentId);

            if (onlyActive)
            {
                query = query.Where(o => o.IsActive);
            }

            var children = await query.ToListAsync();
            int count = children.Count;

            foreach (var child in children)
            {
                count += await CountChildrenRecursive(child.Id, onlyActive);
            }

            return count;
        }

        private async Task NotifyAdmins(string message)
        {
            var admins = await _context.Users
                .Where(u => u.UserRoles!.Any(r => r.Role!.Name == "Admin" || r.Role!.Name == "SuperAdmin"))
                .ToListAsync();
            foreach (var a in admins)
            {
                await _notificationService.SendNotification(a.Id, message, Url.Action("Index","Organization")!);
            }
        }
    }
}