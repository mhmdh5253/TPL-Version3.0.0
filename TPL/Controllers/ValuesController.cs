using Microsoft.AspNetCore.Mvc; // استفاده از MVC برای ساخت کنترلر

namespace TPLWeb.Controllers // فضای نام اصلی پروژه برای کنترلرها
{
    #region Controller & Routing
    [ApiController] // مشخص می‌کند این کلاس یک API Controller است
    [Route("api/[controller]")] // مسیر پایه برای دسترسی به اکشن‌ها
    public class ValuesController : ControllerBase // کنترلر پایه برای API بدون View
    {
        #region Actions
        /// <summary>
        /// جمع دو عدد را برمی‌گرداند.
        /// </summary>
        /// <param name="x">عدد اول (QueryString)</param>
        /// <param name="y">عدد دوم (QueryString)</param>
        /// <returns>نتیجه جمع x و y</returns>
        [HttpGet("index")] // اکشن از نوع GET با مسیر index
        public IActionResult Index(int x, int y) // اکشن ساده برای تست سرویس
        {
            return Ok(x + y); // بازگرداندن 200 OK به همراه نتیجه جمع
        }
        #endregion
    }
    #endregion
}
