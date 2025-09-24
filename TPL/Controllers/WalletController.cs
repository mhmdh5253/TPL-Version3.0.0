using BE; // موجودیت‌های پروژه
using BLL.Wallet; // سرویس کیف پول
using Microsoft.AspNetCore.Authorization; // مجوز دسترسی
using Microsoft.AspNetCore.DataProtection; // حفاظت از داده‌ها
using Microsoft.AspNetCore.Identity; // مدیریت کاربر
using Microsoft.AspNetCore.Mvc; // MVC
using Newtonsoft.Json; // کار با JSON
using System.ComponentModel.DataAnnotations; // اعتبارسنجی‌ها
using System.Net; // ابزارهای شبکه
using System.Security.Claims; // Claims کاربر
using System.Text; // کدگذاری متون

namespace TPLWeb.Controllers
{
    #region Controller & Routing
    [Route("ManageWallet")] // مسیر پایه مدیریت کیف پول
    [Authorize] // نیازمند ورود کاربر
    public class WalletController : Controller
    {
        #region Fields
        private readonly BlWallet _walletRepository; // سرویس عملیات کیف پول
        private readonly ILogger<WalletController> _logger; // لاگر
        private readonly UserManager<ApplicationUser> _userManager; // مدیریت کاربر
        private readonly IConfiguration _configuration; // تنظیمات سیستم
        private readonly IDataProtector _protector; // رمزنگاری داده‌های حساس
        private readonly BLL.Ticketing.BlNotification _notificationService; // اعلان‌ها
        #endregion
        #region Ctor
        public WalletController(
            BlWallet walletRepository,
            ILogger<WalletController> logger,
            UserManager<ApplicationUser> userManager, IConfiguration configuration, IDataProtectionProvider provider,
            BLL.Ticketing.BlNotification notificationService)
        {
            _walletRepository = walletRepository;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _protector = provider.CreateProtector("WalletController.PaymentData");
            _notificationService = notificationService;
        }
        #endregion

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance() // دریافت موجودی کیف پول
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var balance = await _walletRepository.GetBalanceAsync(userId!);
            return Ok(new { Balance = balance, Currency = "Toman" });
        }


        [HttpGet("deposit")]
        public IActionResult Deposit() // صفحه شارژ کیف پول
        {
            return View();
        }

        [HttpPost("deposit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositRequestVm request) // ارسال درخواست پرداخت
        {
            try
            {
                // اعتبارسنجی پیشرفته مدل
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "لطفا اطلاعات را به درستی وارد کنید";
                    return RedirectToAction("Deposit");
                }

                // اعتبارسنجی مقدار مبلغ
                if (!long.TryParse(request.Amount, out var amountValue) || amountValue <= 0)
                {
                    TempData["Error"] = "مبلغ وارد شده نامعتبر است";
                    return RedirectToAction("Deposit");
                }

                // دریافت و اعتبارسنجی کاربر
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "کاربر یافت نشد";
                    return RedirectToAction("Deposit");
                }

                // خواندن امن تنظیمات از کانفیگ
                var zarinpalSettings = _configuration.GetSection("Zarinpal");
                bool isSandbox = zarinpalSettings.GetValue("Mode", "Sandbox")!
                    .Equals("Sandbox", StringComparison.OrdinalIgnoreCase);

                string merchantId = zarinpalSettings["MerchantId"] ??
                    throw new InvalidOperationException("MerchantId not configured");

                string baseUrl = isSandbox ?
                    (zarinpalSettings["SandboxBaseUrl"] ?? "https://sandbox.zarinpal.com") :
                    (zarinpalSettings["ProductionBaseUrl"] ?? "https://api.zarinpal.com");

                // اعتبارسنجی URL درگاه پرداخت
                if (!Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
                {
                    _logger.LogError("Invalid payment gateway URL: {BaseUrl}", baseUrl);
                    throw new InvalidOperationException("آدرس درگاه پرداخت نامعتبر است");
                }

                // ایجاد درخواست پرداخت با اعتبارسنجی بیشتر
                var payload = new
                {
                    merchant_id = merchantId,
                    amount = amountValue * 10, // تبدیل به ریال
                    description = WebUtility.HtmlEncode(request.Description) ?? "شارژ کیف پول",
                    callback_url = Url.Action("VerifyPayment", "Wallet", null, Request.Scheme),
                    metadata = new
                    {
                        mobile = user.PhoneNumber,
                        email = user.Email
                    }
                };

                // ارسال ایمن درخواست به زرین‌پال
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TPL/2.0");

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json");

                // لاگ امنیتی قبل از ارسال
                LogSecurityAction("DepositRequest",
                    $"User: {user.Id}, Amount: {amountValue}",
                    user.Id);

                var response = await httpClient.PostAsync(
                    $"{baseUrl}/pg/v4/payment/request.json",
                    content);

                // پردازش پاسخ با مدیریت خطاهای بهتر
                var result = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse;
                try
                {
                    jsonResponse = JsonConvert.DeserializeObject(result) ??
                        throw new InvalidOperationException("پاسخ نامعتبر از درگاه پرداخت");
                }
                catch (JsonException)
                {
                    _logger.LogError("Invalid JSON response from payment gateway");
                    TempData["Error"] = "پاسخ نامعتبر از درگاه پرداخت";
                    return RedirectToAction("Deposit");
                }

                if (jsonResponse.data.code == 100) // وضعیت موفق
                {
                    string authority = jsonResponse.data.authority.ToString();

                    // رمزنگاری ایمن داده‌های حساس
                    var protectedAmount = _protector.Protect(request.Amount);
                    var protectedAuthority = _protector.Protect(authority);

                    // ذخیره امن در Session
                    HttpContext.Session.SetString("PaymentAmount", protectedAmount);
                    HttpContext.Session.SetString("PaymentAuthority", protectedAuthority);

                    // هدایت کاربر به درگاه پرداخت
                    string paymentUrl = isSandbox
                        ? $"https://sandbox.zarinpal.com/pg/StartPay/{authority}"
                        : $"https://www.zarinpal.com/pg/StartPay/{authority}";

                    // لاگ امنیتی
                    LogSecurityAction("RedirectToPayment",
                        $"User: {user.Id}, Amount: {amountValue}, Authority: {authority}",
                        user.Id);

                    return Redirect(paymentUrl);
                }
                else
                {
                    string errorMessage = jsonResponse.errors?.message ?? "خطا در اتصال به درگاه پرداخت";
                    string errorCode = jsonResponse.data?.code?.ToString() ?? "unknown";

                    _logger.LogError("Payment request failed. Code: {Code}, Message: {Message}",
                        errorCode, errorMessage);

                    TempData["Error"] = $"کد خطا: {errorCode}. {errorMessage}";
                    return RedirectToAction("Deposit");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in deposit request");
                TempData["Error"] = "خطا در پردازش درخواست پرداخت";
                return RedirectToAction("Deposit");
            }
        }

        private void LogSecurityAction(string action, string details, string userId) // ثبت رخدادهای امنیتی
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            _logger.LogInformation($"Security Action: {action} | User: {userId} | " +
                $"IP: {ipAddress} | Details: {details} | UserAgent: {userAgent}");
        }




        [HttpGet("VerifyPayment")]
        public async Task<IActionResult> VerifyPayment([FromQuery] string authority, [FromQuery] string status) // تایید پرداخت برگشتی
        {
            try
            {
                // بررسی وضعیت اولیه
                if (status != "OK")
                {
                    TempData["Error"] = "پرداخت توسط کاربر لغو شد";
                    return RedirectToAction("Deposit");
                }

                // دریافت اطلاعات کاربر
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "کاربر یافت نشد";
                    return RedirectToAction("Deposit");
                }

                var protectedAmount = HttpContext.Session.GetString("PaymentAmount");
                var protectedAuthority = HttpContext.Session.GetString("PaymentAuthority");

                if (string.IsNullOrEmpty(protectedAmount) || string.IsNullOrEmpty(protectedAuthority))
                {
                    TempData["Error"] = "اطلاعات تراکنش نامعتبر است";
                    return RedirectToAction("Deposit");
                }

                var amount = _protector.Unprotect(protectedAmount);
                var savedAuthority = _protector.Unprotect(protectedAuthority);

                // خواندن تنظیمات از کانفیگ
                var zarinpalSettings = _configuration.GetSection("Zarinpal");
                bool isSandbox = zarinpalSettings.GetValue("Mode", "Sandbox")!.Equals("Sandbox", StringComparison.OrdinalIgnoreCase);
                string merchantId = zarinpalSettings.GetValue("MerchantId", "sandbox")!;
                string baseUrl = (isSandbox ? zarinpalSettings.GetValue("SandboxBaseUrl", "https://sandbox.zarinpal.com") : zarinpalSettings.GetValue("ProductionBaseUrl", "https://api.zarinpal.com"))!;

                // ایجاد درخواست تایید
                var verifyPayload = new
                {
                    merchant_id = merchantId,
                    amount = Convert.ToInt32(amount) * 10,
                    authority = authority
                };

                // ارسال درخواست تایید
                using var httpClient = new HttpClient();
                var content = new StringContent(
                    JsonConvert.SerializeObject(verifyPayload),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(
                    $"{baseUrl}/pg/v4/payment/verify.json",
                    content);

                // پردازش پاسخ
                var result = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(result)!;

                if (jsonResponse.data.code == 100) // پرداخت موفق
                {
                    // واریز مبلغ به کیف پول
                    await _walletRepository.DepositAsync(
                        user.Id,
                        Convert.ToDecimal(amount),
                        "شارژ کیف پول از طریق زرین‌پال");




                    // پاک کردن اطلاعات session
                    HttpContext.Session.Remove("PaymentAmount");
                    HttpContext.Session.Remove("PaymentAuthority");
                    TempData["Success"] = $"پرداخت با موفقیت انجام شد. مبلغ {amount} تومان به کیف پول شما اضافه شد.";
                    await _notificationService.SendNotification(user.Id, $"شارژ کیف پول با موفقیت انجام شد: {amount} تومان", Url.Action("GetBalance","Wallet")!);
                    return RedirectToAction("Deposit");
                }
                else
                {
                    string errorMessage = jsonResponse.errors?.message ?? "پرداخت تایید نشد";
                    TempData["Error"] = $"کد خطا: {jsonResponse.data.code}. {errorMessage}";
                    return RedirectToAction("Deposit");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment verification");
                TempData["Error"] = "خطا در پردازش پرداخت";
                return RedirectToAction("Deposit");
            }
        }


        [HttpPost("withdraw")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(WithdrawRequestVm request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
                {
                    return BadRequest(new { Message = "مبلغ وارد شده نامعتبر است" });
                }

                var newBalance = await _walletRepository.WithdrawAsync(
                    userId!,
                    amount,
                    request.Description ?? "برداشت از کیف پول");

                return Ok(new
                {
                    Balance = newBalance,
                    Message = "برداشت با موفقیت انجام شد."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Insufficient balance");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in withdrawal");
                return BadRequest(new { Message = "خطا در انجام عملیات برداشت" });
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int count = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = await _walletRepository.GetTransactionHistoryAsync(userId!, count);
            return Ok(transactions);
        }

        public class DepositRequestVm
        {
            [Display(Name = "مبلغ (ریال)")]
            [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
            [Range(1000, 100000000, ErrorMessage = "مبلغ باید بین 1,000 تا 100,000,000 ریال باشد")]
            [RegularExpression(@"^\d+$", ErrorMessage = "فقط اعداد مجاز هستند")]
            public string? Amount { get; set; }

            [Display(Name = "توضیحات")]
            [StringLength(500)]
            public string? Description { get; set; }
        }

        public class WithdrawRequestVm
        {
            [Display(Name = "مبلغ (ریال)")]
            [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
            [Range(1000, 100000000, ErrorMessage = "مبلغ باید بین 1,000 تا 100,000,000 ریال باشد")]
            public string? Amount { get; set; }

            [Display(Name = "توضیحات")]
            [StringLength(500)]
            public string? Description { get; set; }
        }
    }
    #endregion
}