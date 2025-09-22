using BE;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using TPLWeb.Models;
using TPLWeb.Models.ManageUser;
using TPLWeb.Tools;
using TPLWeb.Services.Sms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace TPLWeb.Controllers
{
    #region ctor
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly RenderViewToString.IViewRenderService _viewRenderService;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        public AccountController(ILogger<AccountController> logger, RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, RenderViewToString.IViewRenderService viewRenderService,
            IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, ISmsSender smsSender,
            IWebHostEnvironment environment,IMemoryCache cache)
        {
            _roleManager = roleManager;
            _logger = logger;
            _userManager = userManager;
            _viewRenderService = viewRenderService;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _smsSender = smsSender;
            _environment = environment;
            _cache = cache;
        }
        #endregion


        /// <summary>
        /// افعال مربوط به پروفایل کاربری و تغییر شماره تلفن از پروفایل
        /// </summary>
        /// <returns></returns>
        #region Profile
        [HttpGet("profile")]
        [Authorize]
        public Task<IActionResult> ProfilePage()
        {

            var u1 = _userManager.Users.Include(x => x.Organization).FirstOrDefault(x => x.Id == _userManager.GetUserId(User));
            if (u1 is null)
            {
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Home"));

            }
            if (!string.IsNullOrEmpty(u1.Email) && u1.EmailConfirmed != true)
            {
                TempData["email"] = "لطفا ایمیل خود را تایید نمایید.";
                u1.EmailConfirmed = false;
                //await SendEmailConfirmationTokenAsync(u1);

            }
            AddViewModel model = new AddViewModel()
            {
                Phone = u1!.PhoneNumber,
                AccountType = u1!.AccountType,
                Address = u1!.Address,
                LastName = u1!.LastName,
                Email = u1!.Email,
                FirstName = u1!.FirstName,
                Ostan = u1!.Ostan,
                Id = u1!.Id,
                CompanyName = u1.CompanyName,
                Emza = u1.Emza,
                HaqEmza = u1.HaqEmza,
                NameEdareh = u1.NameEdareh,
                Semat = u1.Semat,
                UserName = u1.UserName,
                Organization = u1.Organization,
                Avatar = u1.Avatar

            };
            if (u1.PhoneNumber == u1.UserName)
            {
                TempData["res"] = "لطفا اطلاعات کاربری خود را بروزرسانی نمایید";
                model.UserName = "";
            }
            else
            {
                model.UserName = u1.UserName;
            }
            if (!string.IsNullOrEmpty(u1.Email) && u1.EmailConfirmed != true)
            {
                TempData["email"] = "لطفا ایمیل خود را تایید نمایید.";
            }
            return Task.FromResult<IActionResult>(View(model));
        }

        [Authorize]
        [HttpPost("UploadAvatar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile? avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                TempData["res"] = "لطفا یک تصویر معتبر انتخاب کنید";
                return RedirectToAction(nameof(ProfilePage));
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = global::System.IO.Path.GetExtension(avatar.FileName);
            if (!allowedExtensions.Contains(extension))
            {
                TempData["res"] = "فرمت تصویر مجاز نیست";
                return RedirectToAction(nameof(ProfilePage));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var avatarsDir = global::System.IO.Path.Combine(_environment.WebRootPath, "CompanyVariables", "avatars");
            if (!global::System.IO.Directory.Exists(avatarsDir))
            {
                global::System.IO.Directory.CreateDirectory(avatarsDir);
            }

            // delete old file if exists
            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                var oldPath = global::System.IO.Path.Combine(avatarsDir, user.Avatar);
                if (global::System.IO.File.Exists(oldPath))
                {
                    try { global::System.IO.File.Delete(oldPath); } catch { /* ignore */ }
                }
            }

            var safeFileName = $"{user.Id}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = global::System.IO.Path.Combine(avatarsDir, safeFileName);
            using (var stream = new global::System.IO.FileStream(filePath, global::System.IO.FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.Avatar = safeFileName;
            await _userManager.UpdateAsync(user);

            TempData["res"] = "تصویر پروفایل با موفقیت بروزرسانی شد";
            return RedirectToAction(nameof(ProfilePage));
        }

        [Authorize]
        [HttpPost("DeleteAvatar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvatar()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var avatarsDir = global::System.IO.Path.Combine(_environment.WebRootPath, "CompanyVariables", "avatars");
            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                var oldPath = global::System.IO.Path.Combine(avatarsDir, user.Avatar);
                if (global::System.IO.File.Exists(oldPath))
                {
                    try { global::System.IO.File.Delete(oldPath); } catch { /* ignore */ }
                }
                user.Avatar = null;
                await _userManager.UpdateAsync(user);
            }

            TempData["res"] = "تصویر پروفایل حذف شد";
            return RedirectToAction(nameof(ProfilePage));
        }


        //[HttpPost("profile")]
        //public async Task<IActionResult> ProfilePage(ProfileVm model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ModelState.AddModelError("error", "لطفا اطلاعات ورودی را کنترل نمایید");
        //        return View(model);
        //    }
        //    var u1 = await _userManager.FindByNameAsync(User.Identity!.Name!);
        //    u1!.FirstName = model.FirstName;
        //    u1.LastName = model.LastName;
        //    u1.UserName = model.UserName;
        //    u1.AccountType = model.AccountType;
        //    u1.Address = model.Address;
        //    u1.Ostan = model.Ostan;
        //    u1.Email = model.Email;
        //    if (u1.PhoneNumber != model.Phone || u1.PhoneNumberConfirmed == false)
        //    {
        //        u1.PhoneNumberConfirmed = false;
        //        u1.PhoneNumber = model.Phone;
        //        await _userManager.UpdateAsync(u1);
        //        await NewMobileConfirm(u1.Id);
        //        // هدایت به تأیید شماره جدید
        //    }
        //    else
        //    {
        //        u1.PhoneNumber = model.Phone;
        //    }

        //    if (!string.IsNullOrEmpty(u1.Email) && u1.EmailConfirmed != true)
        //    {
        //        u1.EmailConfirmed = false;
        //        // await SendEmailConfirmationTokenAsync(u1);

        //    }
        //    u1.EmailConfirmed = true;
        //    await _userManager.UpdateAsync(u1);
        //    //await _signInManager.RefreshSignInAsync(u1);
        //    await Task.Delay(5000);
        //    TempData["res"] = "بروزرسانی اطلاعات کاربری با موفقیت انجام شد";
        //    return RedirectToAction(nameof(ProfilePage));
        //}

        [HttpGet("ChangePhonenumber")]
        public async Task<IActionResult> NewMobileConfirm(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var mobileCode = await _userManager.GenerateTwoFactorTokenAsync(user!, "Phone");
            ViewBag.IsSent = true;

            //ارسال پیامک فعالسازی حساب
            await _smsSender.SendSmsAsync(mobileCode, user!.PhoneNumber!);

            string mobiletoken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(mobileCode));
            _logger.LogWarning($"#Change User PhoneNumber:{user.PhoneNumber} Datetime:{DateTime.Now}");
            return RedirectToAction("ConfirmMobile", new { phone = user.PhoneNumber, token = mobiletoken });
        }
        #endregion


        /// <summary>
        /// افعال مربوط به کنترل دسترسی غیرمجاز
        /// </summary>
        /// <returns></returns>
        #region AccessDenied
        [HttpGet("accessdenied")]
        public IActionResult AccessDenied()
        {
            if (User.Identity != null)
            {
                _logger.LogWarning(
                    $"#AccessDenied User:{User.Identity.Name} Datetime:{DateTime.Now}");
            }

            return View();
        }

        #endregion




        /// <summary>
        /// افعال مربوط به خروج از حساب کاربری
        /// </summary>
        /// <returns></returns>
        #region logout
        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogWarning($"#Logout User:{User.Identity!.Name} Datetime:{DateTime.Now} Logedout");
            return RedirectToAction("Login", "Account");
        }

        #endregion



        /// <summary>
        /// افعال مربوط به ثبت نام با موبایل
        /// </summary>
        /// <returns></returns>
        #region register

        [HttpGet("phonereg")]
        public IActionResult Register()
        {
            ViewBag.IsSent = false;
            return View();
        }

        [HttpPost("phonereg")]
        public async Task<IActionResult> Register(RegisterWhitPhone model, bool resetpassword = false)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = (ApplicationUser?)null;
                var mobileCode = (string?)null;

                if (!resetpassword)
                {
                    ViewBag.IsSent = false;
                    var checkuser = await _userManager.Users.SingleOrDefaultAsync(u =>
                        u.PhoneNumber == model.Phone || u.UserName == model.Phone);
                    if (checkuser is not null)
                    {
                        ModelState.AddModelError(string.Empty, "شما قبلا با این شماره در سایت ثبت نام نموده اید.");
                        return View(model);

                    }
                    var defaultPassword = GenerateRandomPassword();
                    var result = await _userManager.CreateAsync(new ApplicationUser
                    {
                        FirstName = null,
                        LastName = null,
                        UserName = model.Phone,
                        PhoneNumber = model.Phone,
                    }, defaultPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var err in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, err.Description);
                        }
                        return View(model);
                    }

                    user = await _userManager.FindByNameAsync(model.Phone!);
                    mobileCode = await _userManager.GenerateTwoFactorTokenAsync(user!, "Phone");

                }
                else
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.Phone);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "کاربری با شماره وارد شده یافت نشد");
                        return View(model);
                    }

                    mobileCode = GenerateOtpCode();
                }

                ViewBag.IsSent = true;
                await _smsSender.SendSmsAsync(mobileCode, user!.PhoneNumber!);

                // Store OTP in cache for verification
                var cacheKey = $"OTP_{user.PhoneNumber}";
                _cache.Set(cacheKey, mobileCode, TimeSpan.FromMinutes(5));

                string mobiletoken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(mobileCode));

                _logger.LogWarning($"#{(resetpassword ? "ResetPassword" : "NewUserSignup")} User:{model.Phone} Datetime:{DateTime.Now}");
                if (resetpassword)
                {
                    return Json(new { 
                        success = true, 
                        message = "کد تایید با موفقیت ارسال شد"
                    });
                }
                return RedirectToAction("ConfirmMobile", new { phone = user.PhoneNumber, token = mobiletoken });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error", ex.Message);
                return View(model);
            }
        }
        #endregion





        /// <summary>
        /// متد ارسال تایید پیامک و ورود با موبایل و پیامک
        /// </summary>
        /// <returns></returns>
        #region ConfirmMobile

        [HttpGet("confirmmobile")]
        public IActionResult ConfirmMobile(string phone, string token, bool resetpass = false)
        {
            if (phone == null || token == null)
            {
                return BadRequest();
            }

            try
            {
                // Decode the token to get the actual OTP code
                var decodedBytes = WebEncoders.Base64UrlDecode(token);
                var actualOtp = Encoding.UTF8.GetString(decodedBytes);
                
                var vm = new RegisterWhitPhone
                {
                    Phone = phone,
                    Code = actualOtp // Use the actual OTP code instead of encoded token
                };
                ViewBag.resetpassword = resetpass;
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding OTP token");
                return BadRequest("Invalid token format");
            }
        }


        [HttpPost("confirmmobile")]
        public async Task<IActionResult> ConfirmMobile(RegisterWhitPhone model, bool resetpass)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Phone);
            if (user == null)
            {
                ModelState.AddModelError("", "کاربری یافت نشد");
                return View(model);
            }

            try
            {
                // Verify OTP from cache
                var cacheKey = $"OTP_{model.Phone}";
                var cachedOtp = _cache.Get<string>(cacheKey);
                
                if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != model.SmsCode)
                {
                    ViewBag.resetpassword = false;
                    ModelState.AddModelError(string.Empty, "کد وارد شده معتبر نمی باشد یا منقضی شده است");
                    return View(model);
                }
                
                // Remove OTP from cache after successful verification
                _cache.Remove(cacheKey);

                if (resetpass == false)
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Ostan) || string.IsNullOrEmpty(user.Email) || user.PhoneNumberConfirmed == false)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: true);
                        TempData["alarm"] = "لطفا پروفایل کاربری تان را تکمیل نمایید";
                        return RedirectToAction("ProfilePage", "Account");
                    }
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("ProfilePage", "Account");
                }
                else
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var newPass = GenerateRandomPassword();
                    var res = await _userManager.ResetPasswordAsync(user, token, newPass);
                    await _userManager.UpdateAsync(user);
                    // ارسال رمز عبور جدید با سرویس SMS جدید
                    await _smsSender.SendNormalSmsAsync($"با سلام \n اطلاعات ورود سایت \n نام کاربری: {user.UserName} \n رمزعبور: {newPass}", user.PhoneNumber!);
                    await _signInManager.RefreshSignInAsync(user);
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConfirmMobile");
                ModelState.AddModelError(string.Empty, "خطا در تایید کد. لطفاً دوباره تلاش کنید.");
                return View(model);
            }
        }

        [HttpPost("verifyresetotp")]
        public async Task<IActionResult> VerifyResetOtp(string phone, string smsCode)
        {
            _logger.LogInformation("VerifyResetOtp called with phone: {Phone}, smsCode: {SmsCode}", phone, smsCode);
            
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(smsCode))
            {
                _logger.LogWarning("Phone or smsCode is null or empty");
                return Json(new { success = false, message = "اطلاعات ناقص است" });
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user == null)
            {
                _logger.LogWarning("User not found for phone: {Phone}", phone);
                return Json(new { success = false, message = "کاربری یافت نشد" });
            }

            try
            {
                // Verify OTP from cache
                var cacheKey = $"OTP_{phone}";
                var cachedOtp = _cache.Get<string>(cacheKey);
                
                _logger.LogInformation("Cache key: {CacheKey}, Cached OTP: {CachedOtp}, Received OTP: {ReceivedOtp}", cacheKey, cachedOtp, smsCode);
                
                if (string.IsNullOrEmpty(cachedOtp))
                {
                    _logger.LogWarning("Cached OTP is null or empty for phone: {Phone}", phone);
                    return Json(new { success = false, message = "کد تایید منقضی شده است" });
                }
                
                if (cachedOtp != smsCode)
                {
                    _logger.LogWarning("OTP mismatch for phone: {Phone}. Expected: {Expected}, Received: {Received}", phone, cachedOtp, smsCode);
                    return Json(new { success = false, message = "کد وارد شده معتبر نمی باشد" });
                }
                
                // Remove OTP from cache after successful verification
                _cache.Remove(cacheKey);
                _logger.LogInformation("OTP verified successfully for phone: {Phone}", phone);

                // Generate new password and reset
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var newPass = GenerateRandomPassword();
                var res = await _userManager.ResetPasswordAsync(user, token, newPass);
                
                if (!res.Succeeded)
                {
                    _logger.LogError("Password reset failed: {Errors}", string.Join(", ", res.Errors.Select(e => e.Description)));
                    return Json(new { success = false, message = "خطا در تغییر رمز عبور" });
                }
                
                await _userManager.UpdateAsync(user);
                
                // Send new password via SMS
                var smsMessage = $"با سلام اطلاعات ورود به اتوماسیون مکاتباتی \n نام کاربری: {user.UserName} \n رمزعبور: {newPass}";
                await _smsSender.SendNormalSmsAsync(smsMessage, user.PhoneNumber!);
                
                _logger.LogInformation("Password reset successful for user: {Phone}", phone);
                
                return Json(new { 
                    success = true, 
                    message = "رمز عبور جدید با موفقیت ارسال شد",
                    redirectUrl = Url.Action("Login", "Account")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyResetOtp");
                return Json(new { success = false, message = "خطا در تایید کد. لطفاً دوباره تلاش کنید." });
            }
        }
        [HttpPost("verifyphoneotp")]
        public async Task<IActionResult> VerifyPhoneOtp(string phone, string smsCode)
        {
            _logger.LogInformation("VerifyPhoneOtp called with phone: {Phone}, smsCode: {SmsCode}", phone, smsCode);
            
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(smsCode))
            {
                return Json(new { success = false, message = "اطلاعات ناقص است" });
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user == null)
            {
                return Json(new { success = false, message = "کاربری یافت نشد" });
            }

            try
            {
                // Verify OTP from cache
                var cacheKey = $"OTP_{phone}";
                var cachedOtp = _cache.Get<string>(cacheKey);
                
                if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != smsCode)
                {
                    return Json(new { success = false, message = "کد وارد شده معتبر نمی باشد یا منقضی شده است" });
                }
                
                // Remove OTP from cache after successful verification
                _cache.Remove(cacheKey);

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: true);
                
                return Json(new { 
                    success = true, 
                    message = "ورود با موفقیت انجام شد",
                    redirectUrl = Url.Action("ProfilePage", "Account")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyPhoneOtp");
                return Json(new { success = false, message = "خطا در تایید کد. لطفاً دوباره تلاش کنید." });
            }
        }

        [HttpPost("resendotp")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return Json(new { success = false, message = "شماره تلفن الزامی است" });
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                if (user == null)
                {
                    return Json(new { success = false, message = "کاربری یافت نشد" });
                }

                // Check if enough time has passed since last OTP (2 minutes)
                var lastOtpKey = $"LastOtp_{phoneNumber}";
                var lastOtpTime = _cache.Get<DateTime?>(lastOtpKey);
                
                if (lastOtpTime.HasValue && DateTime.UtcNow.Subtract(lastOtpTime.Value).TotalMinutes < 2)
                {
                    var remainingTime = 120 - (int)DateTime.UtcNow.Subtract(lastOtpTime.Value).TotalSeconds;
                    return Json(new { success = false, message = $"لطفاً {remainingTime} ثانیه دیگر صبر کنید" });
                }

                // Generate new OTP
                var otpCode = GenerateOtpCode();
                
                // Store OTP in cache for verification (5 minutes expiry)
                var cacheKey = $"OTP_{phoneNumber}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));
                
                // Store last OTP time
                _cache.Set(lastOtpKey, DateTime.UtcNow, TimeSpan.FromMinutes(5));
                
                // Send OTP via SmsSender service
                var smsResult = await _smsSender.SendSmsAsync(otpCode, phoneNumber);
                
                if (smsResult.StartsWith("Success:"))
                {
                    return Json(new { success = true, message = "کد تایید جدید ارسال شد" });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در ارسال کد تایید" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP to {PhoneNumber}", phoneNumber);
                return Json(new { success = false, message = "خطا در ارسال مجدد کد تایید" });
            }
        }

        #endregion




        /// <summary>
        /// افعال مربوط به ورود عادی با نام کاربری
        /// </summary>
        /// <returns></returns>
        #region Login

        private async Task FirstStartAsync(LoginVM model)
        {
            // for first app running
            if (model.UserName == "mhmdh5253" && model.Password == "admin@mhmdh5253")
            {
                // Create user
                var result = await _userManager.CreateAsync(new ApplicationUser
                {
                    FirstName = "محمد",
                    LastName = "حبیبی",
                    UserName = model.UserName,
                    Email = "mhmdh5253@gmail.com",
                    PhoneNumber = "09024281355",
                    Ostan = "قم"
                }, "147852036987@Mha");

                if (result.Succeeded)
                {
                    var admin = await _userManager.FindByNameAsync(model.UserName);
                    await _roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
                    await _roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin" });
                    await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });

                    var selectedRoles = new List<string> { "SuperAdmin", "Admin" };
                    await _userManager.AddToRolesAsync(admin!, selectedRoles);

                    admin!.PhoneNumberConfirmed = true;
                    admin.EmailConfirmed = true;
                    await _userManager.UpdateAsync(admin);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (_signInManager.IsSignedIn(User))
                {
                    return RedirectToAction("ProfilePage", "Account");
                }

                if (model.UserName == "mhmdh5253" && model.Password == "admin@mhmdh5253")
                {
                    await FirstStartAsync(model);
                }

                var user = _userManager.Users.Where(u => u.UserName == model.UserName).FirstOrDefault();
                if (user == null || user.LockoutEnabled)
                {
                    ModelState.AddModelError(string.Empty, " کاربری با این مشخصات پیدا نشد یا حساب شما قفل شده است.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.UserName!, model.Password!, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("ProfilePage", "Account");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "حساب کاربری شما قفل شده است");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "تلاش برای ورود نامعتبر است.لطفا دقایقی دیگر مجددا مراجعه نمایید و یا با پشتیبان سایت ارتباط برقرار نمایید.باتشکر");
                return View(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);
                return View(model);
            }
        }

        #endregion




        /// <summary>
        /// ورود کاربر با شماره موبایل
        /// </summary>
        /// <returns></returns>
        #region loginWhiPhone

        [HttpGet("PhoneLogin")]
        public IActionResult LoginWhitPhone(string? returnUrl = "/")
        {
            returnUrl ??= Url.Content("/");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost("phonelogin")]
        public async Task<IActionResult> LoginWhitPhone(LoginWithPhoneVM model, string returnUrl = "/")
        {
            returnUrl ??= Url.Content("/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Phone);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "کاربری با این مشخصات یافت نشد");
                return View(model);
            }

            try
            {
                // Generate OTP code
                var otpCode = GenerateOtpCode();
                
                // Store OTP in cache for verification
                var cacheKey = $"OTP_{user.PhoneNumber}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5)); // 5 minutes expiry
                
                // Send OTP via SmsSender service
                var smsResult = await _smsSender.SendSmsAsync(otpCode, user.PhoneNumber!);
                
                if (!smsResult.StartsWith("Success:"))
                {
                    ModelState.AddModelError(string.Empty, "خطا در ارسال کد تایید. لطفاً دوباره تلاش کنید.");
                    _logger.LogError("Failed to send OTP via SMS service: {Error}", smsResult);
                    return View(model);
                }

                // Encode OTP for URL safety
                var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(otpCode));
                
                // Return JSON response for AJAX
                return Json(new { 
                    success = true, 
                    message = "کد تایید با موفقیت ارسال شد",
                    redirectUrl = Url.Action("ConfirmMobile", new { token, phone = user.PhoneNumber })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP for phone login");
                return Json(new { 
                    success = false, 
                    message = "خطا در ارسال کد تایید. لطفاً دوباره تلاش کنید." 
                });
            }
        }
        
        /// <summary>
        /// Generate a random 6-digit OTP code
        /// </summary>
        private string GenerateOtpCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }



        #endregion



        /// <summary>
        /// بررسی نام کاربری و ایمیل کاربر برای جلوگیری از ثبت نام تکراری
        /// </summary>
        /// <returns></returns>
        #region Remote Validations

        [HttpPost("IsAnyUserName")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IsAnyUserName(string userName)
        {
            var any = await _userManager.Users.AnyAsync(u => u.UserName == userName);
            if (!any)
            {
                return Json(true);
            }

            return Json("نام کاربری مورد نظر از قبل ثبت شده است");
        }

        [HttpPost("IsAnyEmail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IsAnyEmail(string email)
        {
            var any = await _userManager.Users.AnyAsync(u => u.Email == email);
            if (!any)
            {
                return Json(true);
            }

            return Json("ایمیل مورد نظر از قبل ثبت شده است");
        }

        [HttpPost("IsAnyPhone")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IsAnyPhone(string phone)
        {
            var any = await _userManager.Users.AnyAsync(u => u.PhoneNumber == phone);
            if (!any)
            {
                return Json(true);
            }

            return Json("شماره تلفن مورد نظر از قبل ثبت شده است");
        }

        #endregion



        /// <summary>
        /// افعال مربوط به تنظیم مجدد رمز عبور
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        #region reset password
        private static string GenerateOtpCode(int length = 6)
        {
            var random = new Random();
            var otp = "";
            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }
            return otp;
        }

        private static string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string specialChars = "!@#$%^&*";
            Random random = new Random();
            char[] password = new char[length];

            // حداقل یک حرف بزرگ
            password[0] = validChars[random.Next(0, 26)];

            // حداقل یک حرف کوچک
            password[1] = validChars[random.Next(26, 52)];

            // حداقل یک عدد
            password[2] = validChars[random.Next(52, 62)];

            // حداقل یک کاراکتر خاص
            password[3] = specialChars[random.Next(specialChars.Length)];

            // بقیه کاراکترها از مجموعه کامل
            for (int i = 4; i < length; i++)
            {
                string allChars = validChars + specialChars;
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // به هم ریختن ترتیب کاراکترها برای امنیت بیشتر
            for (int i = 0; i < password.Length; i++)
            {
                int j = random.Next(i, password.Length);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

        [HttpGet("resetpassword")]
        public IActionResult ResetPassword()
        {
            return View();
        }
        #endregion




        /// <summary>
        /// حذف و غیر فعال سازی حساب کاربری
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountActivation">null or on if on account will be deactive</param>
        /// <returns></returns>
        #region MyRegion
        [HttpPost("Deactive")]
        public async Task<IActionResult> AccountDeactive(string id, string accountActivation)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(accountActivation))
            {
                TempData["error"] = "در صورتی که درخواست حذف حساب را دارید باید موافقت خود را با زدن تیک حذف حساب اعلام نمایید.";
                return RedirectToAction("ProfilePage", "Account");
            }
            else
            {
                var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
                if (user is not null)
                {
                    var res = await _userManager.SetLockoutEnabledAsync(user, true);
                    if (res.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion




        /// <summary>
        /// ریست پس داخلی
        /// </summary>
        /// <returns></returns>
        #region resetpass
        [Authorize]
        [HttpGet("resetpassaccount")]
        public async Task<IActionResult> ResetPass()
        {

            var u1 = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            var model = new ResetPassModel
            {
                Id = u1!.Id,
                Password = "",
                Password2 = ""
            };
            return View(model);
        }

        [Authorize]
        [HttpPost("resetpassaccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPass(ResetPassModel model)
        {
            //بررسی مدل ولید
            if (!ModelState.IsValid)
            {
                TempData["res"] = "لطفا موارد درخواستی را به صورت کامل تکمیل فرمایید.";
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id!);
            if (user == null)
            {
                TempData["res"] = "خطای داخلی رخ داده است.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password!);
            await _userManager.UpdateAsync(user);
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogWarning($"#ResetPassWord User:{user.UserName} Datetime:{DateTime.Now}");
            TempData["res"] = "تغییر رمز عبور با موفقیت صورت گرفت در صورت امکان مجددا با رمز عبور جدید وارد شوید.";
            return View();
        }

        #endregion



        //فعل مربوط به فعالسازی ایمیل 
        #region ConfirmEmail

        private async Task<IActionResult> SendEmailConfirmationTokenAsync(ApplicationUser user)
        {
            try
            {
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                emailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));

                string? callBackUrl = Url.ActionLink("ConfirmEmail", "Account", new { userID = user.Id, token = emailToken }, Request.Scheme);
                string body = await _viewRenderService.RenderToStringAsync("_RegisterEmailConfirmation", callBackUrl!);

                await _emailSender.SendEmailAsync(new EmailModel(user.Email!, "ایمیل فعالسازی حساب کاربری", body));

                return RedirectToAction("ConfirmEmail", new { userId = user.Id, token = emailToken });
            }
            catch (Exception e)
            {
                _logger.LogError($"Error in sending email confirmation token: {e.Message}");
                return RedirectToAction("ProfilePage", "Account");
            }
        }


        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            ViewBag.IsConfirmed = result.Succeeded ? true : false;

            return View();
        }

        #endregion



    }
}