using Microsoft.AspNetCore.Mvc;

namespace TPLWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// این یکک تست است
        /// </summary>
        /// <returns></returns>
        [HttpGet("index")] // مشخص کردن HTTP verb و Route صحیح
        public IActionResult Index(int x, int y) // بهتر است با حروف بزرگ شروع شود
        {
            return Ok(x + y);
        }
    }
}
