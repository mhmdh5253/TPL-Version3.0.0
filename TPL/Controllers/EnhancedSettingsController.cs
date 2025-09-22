using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PARSGREEN.CORE.RESTful.SMS;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using TPLWeb.Models;
using TPLWeb.Tools;

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class EnhancedSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISmsSender _smsService;
        private readonly IWebHostEnvironment _env;

        public EnhancedSettingsController(IConfiguration configuration, ISmsSender smsService, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _smsService = smsService;
            _env = env;
        }

        public IActionResult Index()
        {
            // Load SMS settings from configuration
            ViewBag.SmsProvider = _configuration["SmsSettings:Provider"] ?? "ParsGreen";
            ViewBag.SmsApiUrl = _configuration["SmsSettings:ApiUrl"] ?? string.Empty;
            ViewBag.SmsBearerToken = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? "";
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
        Provider = string.IsNullOrWhiteSpace(Provider) ? (_configuration["SmsSettings:Provider"] ?? "ParsGreen") : Provider;
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

                // Persist to appsettings.json (only SmsSettings section)
                SaveSmsSettingsToAppSettings(new SmsSettingsDto
                {
                    Provider = Provider,
                    ApiUrl = ApiUrl,
                    BearerToken = BearerToken,
                    ApiKey = BearerToken, // map BearerToken to ApiKey for ParsGreen
                    Username = Username,
                    Password = Password,
                    Sender = Sender,
                    OtpPattern = OtpPattern,
                    IsActive = IsActive
                });

                TempData["SuccessMessage"] = "تنظیمات SMS با موفقیت در appsettings.json ذخیره شد.";
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
                var provider = _configuration["SmsSettings:Provider"] ?? "ParsGreen";
                var apiKey = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? string.Empty;
                var isActive = !string.IsNullOrWhiteSpace(apiKey);

                string statusText = isActive ? "فعال" : "غیرفعال";
                string displayName = provider.Equals("ParsGreen", StringComparison.OrdinalIgnoreCase) ? "🌐 ParsGreen" : "🌐 " + provider;

                if (isActive && provider.Equals("ParsGreen", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var user = new User(apiKey);
                        var json = GetParsGreenCreditText(user); // پاسخ JSON
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            using var doc = JsonDocument.Parse(json);
                            var rial = doc.RootElement.GetProperty("Amount").GetInt64();
                            statusText = $"فعال - موجودی: {rial.ToString("N0", CultureInfo.InvariantCulture)} ریال";
                        }
                    }
                    catch { }
                }

                var providers = new[]
                {
                    new
                    {
                        ProviderName = provider,
                        DisplayName = displayName,
                        IsActive = isActive,
                        Status = statusText,
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

        private static string GetParsGreenCreditText(User user)
        {
            try
            {
                // Try property first
                var prop = user.GetType().GetProperty("Credit", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    var val = prop.GetValue(user);
                    var extracted = ExtractCreditValue(val);
                    if (!string.IsNullOrWhiteSpace(extracted)) return extracted!;

                    if (val is Delegate del)
                    {
                        var res = del.DynamicInvoke();
                        extracted = ExtractCreditValue(res);
                        if (!string.IsNullOrWhiteSpace(extracted)) return extracted!;
                    }

                    var invoke = val?.GetType().GetMethod("Invoke", Type.EmptyTypes);
                    if (invoke != null)
                    {
                        var res2 = invoke.Invoke(val, null);
                        var extracted2 = ExtractCreditValue(res2);
                        if (!string.IsNullOrWhiteSpace(extracted2)) return extracted2!;
                    }
                }

                // Try method Credit()
                var meth = user.GetType().GetMethod("Credit", BindingFlags.Public | BindingFlags.Instance, new Type[0]);
                if (meth != null)
                {
                    var res = meth.Invoke(user, null);
                    var extracted = ExtractCreditValue(res);
                    if (!string.IsNullOrWhiteSpace(extracted)) return extracted!;
                }
            }
            catch { }
            return string.Empty;
        }

        private static string? ExtractCreditValue(object? obj)
        {
            if (obj == null) return null;

            // If numeric or string, return directly
            switch (obj)
            {
                case string s:
                    return s;
                case int or long or float or double or decimal:
                    return Convert.ToString(obj, System.Globalization.CultureInfo.InvariantCulture);
            }

            // Try common property names on response object
            try
            {
                var t = obj.GetType();
                foreach (var name in new[] { "Credit", "credit", "Balance", "balance", "Remain", "remain" })
                {
                    var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p != null)
                    {
                        var v = p.GetValue(obj);
                        if (v is not null)
                        {
                            return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            catch { }

            // Fallback: compact JSON (avoid delegate type names)
            try { return JsonSerializer.Serialize(obj); } catch { return obj.ToString(); }
        }

        private void SaveSmsSettingsToAppSettings(SmsSettingsDto dto)
        {
            var path = Path.Combine(_env.ContentRootPath, "appsettings.json");
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException("appsettings.json not found", path);

            var json = System.IO.File.ReadAllText(path);
            var root = JsonNode.Parse(json)!.AsObject();

            if (!root.TryGetPropertyValue("SmsSettings", out var smsNode) || smsNode is null)
            {
                smsNode = new JsonObject();
                root["SmsSettings"] = smsNode;
            }

            var sms = smsNode.AsObject();
            if (!string.IsNullOrWhiteSpace(dto.Provider)) sms["Provider"] = dto.Provider;
            if (!string.IsNullOrWhiteSpace(dto.ApiUrl)) sms["ApiUrl"] = dto.ApiUrl;
            if (!string.IsNullOrWhiteSpace(dto.BearerToken)) sms["BearerToken"] = dto.BearerToken;
            if (!string.IsNullOrWhiteSpace(dto.ApiKey)) sms["ApiKey"] = dto.ApiKey; // for ParsGreen
            if (!string.IsNullOrWhiteSpace(dto.Username)) sms["Username"] = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Password)) sms["Password"] = dto.Password;
            if (!string.IsNullOrWhiteSpace(dto.Sender)) sms["Sender"] = dto.Sender;
            if (!string.IsNullOrWhiteSpace(dto.OtpPattern)) sms["OtpPattern"] = dto.OtpPattern;
            sms["IsActive"] = dto.IsActive;

            var updated = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(path, updated);
        }

        private sealed class SmsSettingsDto
        {
            public string? Provider { get; set; }
            public string? ApiUrl { get; set; }
            public string? BearerToken { get; set; }
            public string? ApiKey { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Sender { get; set; }
            public string? OtpPattern { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
