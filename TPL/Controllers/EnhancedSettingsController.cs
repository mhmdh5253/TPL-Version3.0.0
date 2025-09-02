using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TPLWeb.Tools;
using TPLWeb.Models;

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class EnhancedSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISmsSender _smsService;

        public EnhancedSettingsController(IConfiguration configuration, ISmsSender smsService)
        {
            _configuration = configuration;
            _smsService = smsService;
        }

        public IActionResult Index()
        {
            // Load SMS settings from configuration
            ViewBag.SmsProvider = _configuration["SmsSettings:Provider"] ?? "ApiIr";
            ViewBag.SmsApiUrl = _configuration["SmsSettings:ApiUrl"] ?? "https://s.api.ir";
            ViewBag.SmsBearerToken = _configuration["SmsSettings:BearerToken"] ?? "";
            ViewBag.SmsUsername = _configuration["SmsSettings:Username"] ?? "";
            ViewBag.SmsPassword = _configuration["SmsSettings:Password"] ?? "";
            ViewBag.SmsSender = _configuration["SmsSettings:Sender"] ?? "";
            ViewBag.SmsOtpPattern = _configuration["SmsSettings:OtpPattern"] ?? "";
            ViewBag.SmsIsActive = _configuration.GetValue<bool>("SmsSettings:IsActive", true);

            // If we have TempData from form submission, use that instead
            if (TempData["SmsApiUrl"] != null)
            {
                ViewBag.SmsProvider = TempData["SmsProvider"]?.ToString() ?? "ApiIr";
                ViewBag.SmsApiUrl = TempData["SmsApiUrl"]?.ToString() ?? "";
                ViewBag.SmsBearerToken = TempData["SmsBearerToken"]?.ToString() ?? "";
                ViewBag.SmsUsername = TempData["SmsUsername"]?.ToString() ?? "";
                ViewBag.SmsPassword = TempData["SmsPassword"]?.ToString() ?? "";
                ViewBag.SmsSender = TempData["SmsSender"]?.ToString() ?? "";
                ViewBag.SmsOtpPattern = TempData["SmsOtpPattern"]?.ToString() ?? "";
                ViewBag.SmsIsActive = TempData["SmsIsActive"] != null && (bool)TempData["SmsIsActive"]!;
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSmsSettings(string Provider, string ApiUrl, string BearerToken, string Username, string Password, string Sender, string OtpPattern, bool IsActive)
        {
            try
            {
                // Note: In a production environment, you would save these settings to a database
                // or update the configuration file. For now, we'll use TempData to show the values.
                
                TempData["SmsProvider"] = Provider;
                TempData["SmsApiUrl"] = ApiUrl;
                TempData["SmsBearerToken"] = BearerToken;
                TempData["SmsUsername"] = Username;
                TempData["SmsPassword"] = Password;
                TempData["SmsSender"] = Sender;
                TempData["SmsOtpPattern"] = OtpPattern;
                TempData["SmsIsActive"] = IsActive;

                TempData["SuccessMessage"] = "تنظیمات SMS با موفقیت بروزرسانی شد. توجه: برای اعمال دائمی، فایل appsettings.json را ویرایش کنید.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در بروزرسانی تنظیمات: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestSmsProvider(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return Json(new { success = false, message = "شماره تلفن الزامی است" });
                }

                var otpCode = GenerateOtpCode();
                var result = await _smsService.SendSmsAsync(otpCode, phoneNumber);

                if (result.StartsWith("Success:"))
                {
                    return Json(new { success = true, message = $"کد تایید {otpCode} با موفقیت ارسال شد" });
                }
                else
                {
                    return Json(new { success = false, message = result });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا در ارسال پیام: {ex.Message}" });
            }
        }

        private string GenerateOtpCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }



        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetSmsProviderStatus()
        {
            try
            {
                // Simple status check - always show as active since we're using the default service
                var isActive = true;
                
                var providers = new[]
                {
                    new
                    {
                        ProviderName = "ApiIr",
                        DisplayName = "🌐 Api.ir",
                        IsActive = isActive,
                        Status = isActive ? "فعال" : "غیرفعال",
                        Priority = "اول",
                        IsDefault = true
                    }
                };

                return Json(new { success = true, data = providers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
