using Microsoft.AspNetCore.Mvc; // ارجاع به MVC برای کنترلر و اکشن‌ها
using System.Diagnostics; // برای دریافت اطلاعات Activity فعلی
using TPLWeb.Models; // مدل‌های مربوط به View ها

namespace TPLWeb.Controllers // فضای نام کنترلرهای وب
{
    #region Controller
    public class HomeController : Controller // کنترلر اصلی صفحه خانه
    {
        #region Fields
        private readonly ILogger<HomeController> _logger; // لاگر جهت ثبت رخدادها
        #endregion

        #region Ctor
        public HomeController(ILogger<HomeController> logger) // سازنده با تزریق وابستگی لاگر
        {
            _logger = logger; // انتساب لاگر
        }
        #endregion

        #region Actions
        [HttpGet]
        public IActionResult Index() // اکشن صفحه اصلی
        {
            return View(); // بازگرداندن View پیش‌فرض Index
            //return RedirectToAction("Login", "Account");
        }


        [HttpGet("privacy/{statusCode}")]
        public IActionResult Privacy(int statusCode) // نمایش Privacy با کد وضعیت به عنوان مدل
        {
            return View(statusCode); // پاس‌دادن statusCode به View
        }



        [Route("documentation")]
        public IActionResult Documentation() // نمایش مستندات
        {
            return View(); // بازگرداندن View مستندات
        }


        [HttpGet("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() // نمایش صفحه خطا به همراه شناسه درخواست
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // ساخت مدل خطا
        }
        #endregion

    }
    #endregion
}