using BE;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPLWeb.Tools;

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("rolemanagement")]
    public class RoleManagementController : Controller
    {
        private readonly ControllerActionService _controllerActionService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BLL.Ticketing.BlNotification _notificationService;
        public RoleManagementController(ControllerActionService controllerActionService, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, BLL.Ticketing.BlNotification notificationService)
        {
            _controllerActionService = controllerActionService;
            _roleManager = roleManager;
            _userManager = userManager;
            _notificationService = notificationService;
        }


        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            await Task.Delay(10);
            return View("AssignRoleIndex");
        }
        [HttpPost("AssignRoleIndex")]
        public async Task<IActionResult> AssignRoleIndex(string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                var roleClaims = await _roleManager.GetClaimsAsync((await _roleManager.FindByNameAsync(role))!);
                var controllerActions = _controllerActionService.GetAllControllerActions();
                await Task.Delay(100);
                ViewBag.RoleName = role;
                ViewBag.RoleClaims = roleClaims;
                ViewBag.rolename = role;
                return View(controllerActions);

            }
            return View();

        }

        [HttpPost("assignroleaccess")]
        public async Task<IActionResult> AssignRoleAccess(string roleName, List<ControllerActions> controllerActions)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["firstload"] = false;
                return NotFound($"Role {roleName} not found");

            }
            var roleClaims = await _roleManager.GetClaimsAsync(role);

            // حذف همه دسترسی‌های نقش
            foreach (var claim in roleClaims)
            {
                if (claim.Type == "Permission")
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            await Task.Delay(200);

            foreach (var controller in controllerActions)
            {
                if (controller.Actions != null)
                {
                    foreach (var action in controller.Actions)
                    {
                        var claim = new Claim("Permission", $"{controller.ControllerName}:{action}");
                        await _roleManager.AddClaimAsync(role, claim);
                    }
                }

            }

            // inform users in this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            foreach (var u in usersInRole)
            {
                await _notificationService.SendNotification(u.Id, $"دسترسی‌های نقش '{roleName}' به‌روزرسانی شد", Url.Action("Index","Home")!);
            }
            TempData["firstload"] = false;
            TempData["asignrolemassege"] = true;
            return RedirectToAction("Index", controllerName: "ManageRole");
        }
    }
}
