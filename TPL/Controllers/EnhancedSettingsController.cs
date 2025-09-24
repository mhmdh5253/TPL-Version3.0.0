using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PARSGREEN.CORE.RESTful.SMS;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography; // برای تولید امن کد OTP
using System.Text.Json;
using System.Text.Json.Nodes;
using TPLWeb.Tools; // دسترسی به ISmsSender

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class EnhancedSettingsController : Controller // کنترلر مدیریت تنظیمات پیشرفته SMS
    {
        #region Fields
        private readonly IConfiguration _configuration; // پیکربندی برنامه (خواندن appsettings)
        private readonly ISmsSender _smsService; // سرویس ارسال پیامک
        private readonly IWebHostEnvironment _env; // محیط میزبانی برای مسیر ریشه محتوا
        #endregion
        #region Ctor
        public EnhancedSettingsController(IConfiguration configuration, ISmsSender smsService, IWebHostEnvironment env) // سازنده جهت تزریق وابستگی‌ها
        {
            _configuration = configuration; // مقداردهی پیکربندی برنامه
            _smsService = smsService; // مقداردهی سرویس پیامک
            _env = env; // مقداردهی محیط میزبانی
        }
        #endregion

        #region Actions
        public IActionResult Index() // نمایش صفحه تنظیمات و بارگذاری مقادیر از پیکربندی
        {
            ViewBag.SmsProvider = _configuration["SmsSettings:Provider"] ?? "ParsGreen"; // نام ارائه‌دهنده پیامک
            ViewBag.SmsApiUrl = _configuration["SmsSettings:ApiUrl"] ?? string.Empty; // آدرس API ارائه‌دهنده
            ViewBag.SmsBearerToken = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? ""; // کلید دسترسی یا توکن
            ViewBag.SmsUsername = _configuration["SmsSettings:Username"] ?? ""; // نام‌کاربری سرویس
            ViewBag.SmsPassword = _configuration["SmsSettings:Password"] ?? ""; // رمز عبور سرویس
            ViewBag.SmsSender = _configuration["SmsSettings:Sender"] ?? ""; // شماره/شناسه فرستنده پیامک
            ViewBag.SmsOtpPattern = _configuration["SmsSettings:OtpPattern"] ?? ""; // الگوی پیام OTP
            ViewBag.SmsIsActive = _configuration.GetValue<bool>("SmsSettings:IsActive", true); // وضعیت فعال بودن ارسال پیامک

            if (TempData["SmsApiUrl"] != null) // در صورت برگشت از ثبت فرم، مقادیر TempData اولویت دارند
            {
                ViewBag.SmsProvider = TempData["SmsProvider"]?.ToString() ?? "ApiIr"; // ارائه‌دهنده انتخاب‌شده
                ViewBag.SmsApiUrl = TempData["SmsApiUrl"]?.ToString() ?? ""; // آدرس API از TempData
                ViewBag.SmsBearerToken = TempData["SmsBearerToken"]?.ToString() ?? ""; // توکن از TempData
                ViewBag.SmsUsername = TempData["SmsUsername"]?.ToString() ?? ""; // نام‌کاربری از TempData
                ViewBag.SmsPassword = TempData["SmsPassword"]?.ToString() ?? ""; // رمز عبور از TempData
                ViewBag.SmsSender = TempData["SmsSender"]?.ToString() ?? ""; // فرستنده از TempData
                ViewBag.SmsOtpPattern = TempData["SmsOtpPattern"]?.ToString() ?? ""; // الگو از TempData
                ViewBag.SmsIsActive = TempData["SmsIsActive"] != null && (bool)TempData["SmsIsActive"]!; // وضعیت فعال بودن از TempData
            }

            return View(); // نمایش ویو تنظیمات
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSmsSettings(string Provider, string ApiUrl, string BearerToken, string Username, string Password, string Sender, string OtpPattern, bool IsActive) // ثبت تغییرات تنظیمات SMS
        {
            try
            {
                Provider = string.IsNullOrWhiteSpace(Provider) ? (_configuration["SmsSettings:Provider"] ?? "ParsGreen") : Provider; // پیش‌فرض ارائه‌دهنده در صورت خالی بودن

                TempData["SmsProvider"] = Provider; // ذخیره موقت نام ارائه‌دهنده
                TempData["SmsApiUrl"] = ApiUrl; // ذخیره موقت آدرس API
                TempData["SmsBearerToken"] = BearerToken; // ذخیره موقت توکن دسترسی
                TempData["SmsUsername"] = Username; // ذخیره موقت نام‌کاربری
                TempData["SmsPassword"] = Password; // ذخیره موقت رمز عبور
                TempData["SmsSender"] = Sender; // ذخیره موقت فرستنده
                TempData["SmsOtpPattern"] = OtpPattern; // ذخیره موقت الگوی OTP
                TempData["SmsIsActive"] = IsActive; // ذخیره موقت وضعیت فعال بودن

                SaveSmsSettingsToAppSettings(new SmsSettingsDto // ذخیره در appsettings.json فقط در بخش SmsSettings
                {
                    Provider = Provider, // نام ارائه‌دهنده
                    ApiUrl = ApiUrl, // آدرس API
                    BearerToken = BearerToken, // توکن دسترسی
                    ApiKey = BearerToken, // نگاشت به ApiKey برای ParsGreen
                    Username = Username, // نام‌کاربری سرویس
                    Password = Password, // رمز عبور سرویس
                    Sender = Sender, // فرستنده پیامک
                    OtpPattern = OtpPattern, // الگوی OTP
                    IsActive = IsActive // وضعیت فعال بودن سرویس
                });

                TempData["SuccessMessage"] = "تنظیمات SMS با موفقیت در appsettings.json ذخیره شد."; // پیام موفقیت
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در بروزرسانی تنظیمات: {ex.Message}"; // پیام خطا در صورت استثنا
            }

            return RedirectToAction(nameof(Index)); // بازگشت به صفحه تنظیمات
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestSmsProvider(string phoneNumber) // ارسال پیام تستی برای بررسی ارائه‌دهنده
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber)) // اعتبارسنجی شماره تلفن
                {
                    return Json(new { success = false, message = "شماره تلفن الزامی است" }); // خطا در صورت خالی بودن
                }

                var otpCode = GenerateOtpCode(); // تولید کد تایید
                var result = await _smsService.SendSmsAsync(otpCode, phoneNumber); // ارسال پیامک حاوی کد

                if (result.StartsWith("Success:")) // بررسی موفقیت‌آمیز بودن ارسال
                {
                    return Json(new { success = true, message = $"کد تایید {otpCode} با موفقیت ارسال شد" }); // پاسخ موفق
                }
                else
                {
                    return Json(new { success = false, message = result }); // بازگرداندن پیام خطا از سرویس
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا در ارسال پیام: {ex.Message}" }); // خطا در فرآیند ارسال
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetSmsProviderStatus() // دریافت وضعیت فعلی ارائه‌دهنده و موجودی احتمالی
        {
            try
            {
                var provider = _configuration["SmsSettings:Provider"] ?? "ParsGreen"; // نام ارائه‌دهنده از تنظیمات
                var apiKey = _configuration["SmsSettings:ApiKey"] ?? _configuration["SmsSettings:BearerToken"] ?? string.Empty; // کلید دسترسی
                var isActive = !string.IsNullOrWhiteSpace(apiKey); // فعال بودن بر اساس وجود کلید

                string statusText = isActive ? "فعال" : "غیرفعال"; // متن وضعیت
                string displayName = provider.Equals("ParsGreen", StringComparison.OrdinalIgnoreCase) ? "🌐 ParsGreen" : "🌐 " + provider; // نام قابل نمایش

                if (isActive && provider.Equals("ParsGreen", StringComparison.OrdinalIgnoreCase)) // اگر ارائه‌دهنده پارس‌گرین و کلید معتبر است
                {
                    try
                    {
                        var user = new User(apiKey); // نمونه کاربر سرویس ParsGreen
                        var json = GetParsGreenCreditText(user); // دریافت پاسخ اعتبار حساب به صورت JSON
                        if (!string.IsNullOrWhiteSpace(json)) // اگر پاسخی وجود دارد
                        {
                            using var doc = JsonDocument.Parse(json); // تجزیه JSON
                            var rial = doc.RootElement.GetProperty("Amount").GetInt64(); // استخراج مبلغ موجودی
                            statusText = $"فعال - موجودی: {rial.ToString("N0", CultureInfo.InvariantCulture)} ریال"; // تنظیم متن وضعیت با موجودی
                        }
                    }
                    catch { } // چشم‌پوشی از خطاهای احتمالی ParsGreen
                }

                var providers = new[] // آماده‌سازی لیست ارائه‌دهندگان برای UI
                {
                    new
                    {
                        ProviderName = provider, // نام ارائه‌دهنده
                        DisplayName = displayName, // نام قابل نمایش
                        IsActive = isActive, // وضعیت فعال بودن
                        Status = statusText, // متن وضعیت
                        Priority = "اول", // اولویت نمایش
                        IsDefault = true // به عنوان پیش‌فرض
                    }
                };

                return Json(new { success = true, data = providers }); // بازگرداندن نتیجه موفق همراه با داده‌ها
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }); // بازگرداندن خطا در صورت بروز استثنا
            }
        }
        #endregion

        #region Helpers
        private static string GenerateOtpCode() // تولید امن کد تایید ۶ رقمی با RNG
        {
            return RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString(CultureInfo.InvariantCulture); // عدد تصادفی امن در بازه 100000..999999
        }

        private static string GetParsGreenCreditText(User user) // تلاش برای دریافت موجودی حساب ParsGreen با بازتاب
        {
            try
            {
                var prop = user.GetType().GetProperty("Credit", BindingFlags.Public | BindingFlags.Instance); // جستجوی پراپرتی Credit
                if (prop != null) // در صورت وجود پراپرتی
                {
                    var val = prop.GetValue(user); // مقدار پراپرتی
                    var extracted = ExtractCreditValue(val); // استخراج مقدار قابل استفاده
                    if (!string.IsNullOrWhiteSpace(extracted)) return extracted!; // در صورت موفقیت برگردان

                    if (val is Delegate del) // اگر مقدار نماینده (delegate) باشد
                    {
                        var res = del.DynamicInvoke(); // فراخوانی داینامیک
                        extracted = ExtractCreditValue(res); // استخراج مقدار
                        if (!string.IsNullOrWhiteSpace(extracted)) return extracted!; // برگرداندن در صورت موفقیت
                    }

                    var invoke = val?.GetType().GetMethod("Invoke", Type.EmptyTypes); // تلاش برای یافتن متد Invoke بدون پارامتر
                    if (invoke != null) // در صورت وجود متد
                    {
                        var res2 = invoke.Invoke(val, null); // فراخوانی متد
                        var extracted2 = ExtractCreditValue(res2); // استخراج مقدار
                        if (!string.IsNullOrWhiteSpace(extracted2)) return extracted2!; // برگرداندن در صورت موفقیت
                    }
                }

                var meth = user.GetType().GetMethod("Credit", BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()); // تلاش برای یافتن متد Credit()
                if (meth != null) // اگر متد پیدا شد
                {
                    var res = meth.Invoke(user, null); // فراخوانی متد
                    var extracted = ExtractCreditValue(res); // استخراج مقدار
                    if (!string.IsNullOrWhiteSpace(extracted)) return extracted!; // برگرداندن مقدار استخراج‌شده
                }
            }
            catch { } // نادیده گرفتن خطاهای بازتاب
            return string.Empty; // در صورت عدم موفقیت مقدار خالی
        }

        private static string? ExtractCreditValue(object? obj) // استخراج عدد/رشته موجودی از انواع مختلف پاسخ
        {
            if (obj == null) return null; // اگر مقدار تهی است

            switch (obj) // بررسی نوع مقدار
            {
                case string s:
                    return s; // بازگرداندن رشته به همان صورت
                case int or long or float or double or decimal:
                    return Convert.ToString(obj, CultureInfo.InvariantCulture); // تبدیل مقدار عددی به رشته با فرهنگ ثابت
            }

            try // جستجو برای پراپرتی‌های رایج در پاسخ
            {
                var t = obj.GetType(); // نوع شیء پاسخ
                foreach (var name in new[] { "Credit", "credit", "Balance", "balance", "Remain", "remain" }) // نام‌های محتمل
                {
                    var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase); // گرفتن پراپرتی با بی‌توجهی به حروف بزرگ/کوچک
                    if (p != null) // اگر پراپرتی یافت شد
                    {
                        var v = p.GetValue(obj); // مقدار پراپرتی
                        if (v is not null) // اگر مقدار دارد
                        {
                            return Convert.ToString(v, CultureInfo.InvariantCulture); // تبدیل به رشته استاندارد
                        }
                    }
                }
            }
            catch { } // نادیده گرفتن خطا

            try { return JsonSerializer.Serialize(obj); } catch { return obj.ToString(); } // بازگشت به JSON فشرده یا ToString
        }

        private void SaveSmsSettingsToAppSettings(SmsSettingsDto dto) // ذخیره تنظیمات در فایل appsettings.json
        {
            var path = Path.Combine(_env.ContentRootPath, "appsettings.json"); // مسیر فایل تنظیمات در ریشه محتوا
            if (!System.IO.File.Exists(path)) // بررسی وجود فایل
                throw new FileNotFoundException("appsettings.json not found", path); // پرتاب خطا در صورت عدم وجود

            var json = System.IO.File.ReadAllText(path); // خواندن محتوای فعلی فایل
            var root = JsonNode.Parse(json)!.AsObject(); // تجزیه JSON و تبدیل به شیء قابل ویرایش

            if (!root.TryGetPropertyValue("SmsSettings", out var smsNode) || smsNode is null) // بررسی وجود بخش SmsSettings
            {
                smsNode = new JsonObject(); // ایجاد شیء جدید برای بخش تنظیمات SMS
                root["SmsSettings"] = smsNode; // افزودن بخش به ریشه تنظیمات
            }

            var sms = smsNode.AsObject(); // تبدیل به JsonObject برای دسترسی ساده
            if (!string.IsNullOrWhiteSpace(dto.Provider)) sms["Provider"] = dto.Provider; // ثبت نام ارائه‌دهنده
            if (!string.IsNullOrWhiteSpace(dto.ApiUrl)) sms["ApiUrl"] = dto.ApiUrl; // ثبت آدرس API
            if (!string.IsNullOrWhiteSpace(dto.BearerToken)) sms["BearerToken"] = dto.BearerToken; // ثبت توکن یا کلید
            if (!string.IsNullOrWhiteSpace(dto.ApiKey)) sms["ApiKey"] = dto.ApiKey; // نگاشت ApiKey برای ParsGreen
            if (!string.IsNullOrWhiteSpace(dto.Username)) sms["Username"] = dto.Username; // ثبت نام‌کاربری
            if (!string.IsNullOrWhiteSpace(dto.Password)) sms["Password"] = dto.Password; // ثبت رمز عبور
            if (!string.IsNullOrWhiteSpace(dto.Sender)) sms["Sender"] = dto.Sender; // ثبت فرستنده
            if (!string.IsNullOrWhiteSpace(dto.OtpPattern)) sms["OtpPattern"] = dto.OtpPattern; // ثبت الگوی OTP
            sms["IsActive"] = dto.IsActive; // ثبت وضعیت فعال بودن

            var updated = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }); // تولید JSON مرتب شده
            System.IO.File.WriteAllText(path, updated); // نوشتن تغییرات در فایل
        }

        private sealed class SmsSettingsDto // مدل داخلی برای نگهداری تنظیمات SMS
        {
            public string? Provider { get; set; } // نام ارائه‌دهنده
            public string? ApiUrl { get; set; } // آدرس API
            public string? BearerToken { get; set; } // توکن دسترسی
            public string? ApiKey { get; set; } // کلید API (در برخی سرویس‌ها)
            public string? Username { get; set; } // نام‌کاربری سرویس
            public string? Password { get; set; } // رمز عبور سرویس
            public string? Sender { get; set; } // شناسه/شماره فرستنده
            public string? OtpPattern { get; set; } // الگوی پیام OTP
            public bool IsActive { get; set; } // وضعیت فعال بودن سرویس
        }
        #endregion
    }
}
