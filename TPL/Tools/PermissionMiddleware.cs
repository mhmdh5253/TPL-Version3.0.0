using BE;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace TPLWeb.Tools
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<ControllerAction> _mainControllers;
        private readonly List<string> _excludedPaths;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
            _mainControllers = GetControllersAndActions();
            _excludedPaths = new List<string>
            {
                "/favicon.ico",
                "/robots.txt",
                "/Account/AccessDenied", // اضافه کردن مسیر دسترسی ممنوع به لیست استثناها
                "/Account/Login",
                "/Account/Logout",
                "/account/phonelogin",
                "/account/register",
                "/account/forgotpassword",
                "/account/resetpassword",
                "/account/confirmemail",
                "/account/verifyphoneotp",
                "/account/resendotp",
                "/account/phonereg",
                "/account/verifyresetotp",
                "/account/confirmmobile",
                "/account/profile",
                "/account/uploadavatar",
                "/account/deleteavatar",
                "/account/changephonenumber",
                "/account/isanyusername",
                "/account/isanyemail",
                "/account/isanyphone",
                "/account/resetpassaccount",
                "/account/deactive",
                "/home",
                "/home/index",
                "/error",
                "/css",
                "/js",
                "/images",
                "/lib"
            };
        }

        private List<ControllerAction> GetControllersAndActions()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .Select(type => new ControllerAction
                {
                    ControllerName = type.Name.Replace("Controller", ""),
                    Actions = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                        .Where(m => !m.GetCustomAttributes(typeof(NonActionAttribute), true).Any())
                        .Select(m => m.Name)
                        .Distinct()
                        .ToList()
                })
                .Where(c => c.Actions != null && c.Actions.Any()) // حذف کنترلرهای بدون اکشن
                .ToList();
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            try
            {
                var path = context.Request.Path.ToString().ToLower();

                // بررسی مسیرهای استثنا
                if (_excludedPaths.Any(e => path.Equals(e, StringComparison.OrdinalIgnoreCase)))
                {
                    await _next(context);
                    return;
                }

                // اگر کاربر احراز هویت نشده، به صفحه لاگین هدایت شود
                if (!context.User.Identity!.IsAuthenticated)
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }

                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }

                // کاربران SuperAdmin نیازی به بررسی مجوز ندارند
                if (context.User.IsInRole("SuperAdmin"))
                {
                    await _next(context);
                    return;
                }

                var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length >= 1)
                {
                    var controller = segments[0];
                    var action = segments.Length > 1 ? segments[1] : "index";

                    var controllerAction = _mainControllers.FirstOrDefault(c =>
                        c.ControllerName.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                        c.Actions.Contains(action, StringComparer.OrdinalIgnoreCase));

                    if (controllerAction != null)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        bool hasPermission = false;

                        foreach (var roleName in roles)
                        {
                            var role = await roleManager.FindByNameAsync(roleName);
                            if (role != null)
                            {
                                var roleClaims = await roleManager.GetClaimsAsync(role);
                                if (roleClaims.Any(c =>
                                    c.Type == "Permission" &&
                                    c.Value.Equals($"{controller.ToLower()}:{action.ToLower()}", StringComparison.OrdinalIgnoreCase)))
                                {
                                    hasPermission = true;
                                    break;
                                }
                            }
                        }

                        if (!hasPermission)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.Redirect("/Account/AccessDenied");
                            return;
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception)
            {
                // در صورت بروز خطا، کاربر را به صفحه لاگین هدایت می‌کنیم
                context.Response.Redirect("/Account/Login");
            }
        }
    }

    public class ControllerAction
    {
        public string ControllerName { get; set; }
        public List<string> Actions { get; set; }

        public ControllerAction()
        {
            ControllerName = string.Empty;
            Actions = new List<string>();
        }
    }
}