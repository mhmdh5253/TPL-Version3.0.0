using BE.LetterAutomation;
using BLL.LetterAutomation;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using BE;

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("kelaseh")]
    public class KelasehnamehsController : Controller
    {
        private readonly Blkelaseh _context;
        private readonly BlLetter _letter;
        private readonly Db _dbcon;
        private readonly UserManager<ApplicationUser> _userManager;

        public KelasehnamehsController(
            Blkelaseh context,
            BlLetter letter,
            Db dbcon,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _letter = letter;
            _dbcon = dbcon;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User)!;
        }

        [HttpGet("getAll")]
        public async Task<List<Kelasehnameh>> GetAll()
        {
            var userId = GetCurrentUserId();
            return (await _context.GetAllKelasehnamehsAsync(userId)).ToList();
        }

        [HttpGet("Kelasehnameha")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var kls = await _context.GetAllKelasehnamehsAsync(userId);

            if (!kls.Any())
            {
                var res = kls.FirstOrDefault(x => x.NameKelaseh == "اصلی");
                if (res == null)
                {

                    Random random = new Random();
                    await _context.CreateKelasehnamehAsync(new Kelasehnameh()
                    {
                        CodeKelaseh = random.Next(99999999),
                        NameKelaseh = "اصلی",
                        UserId = userId
                    });

                }
            }

            return View(await _context.GetAllKelasehnamehsAsync(userId));
        }

        [HttpGet("Kelasehnamehs/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var kelasehnameh = await _context.GetKelasehnamehByIdAsync(id, userId);

            if (kelasehnameh == null)
            {
                return NotFound();
            }

            return View(kelasehnameh);
        }

        [HttpGet("CreateKelaseh")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("CreateKelaseh")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CodeKelaseh,NameKelaseh")] Kelasehnameh kelasehnameh)
        {
            var userId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                var klass = await _context.GetAllKelasehnamehsAsync(userId);

                if (klass.Any(x => x.NameKelaseh == kelasehnameh.NameKelaseh || x.CodeKelaseh == kelasehnameh.CodeKelaseh))
                {
                    ModelState.AddModelError("", "این کلاسه نامه از قبل موجود می باشد");
                    return View(kelasehnameh);
                }

                kelasehnameh.UserId = userId;
                await _context.CreateKelasehnamehAsync(kelasehnameh);
                return RedirectToAction(nameof(Index));
            }

            return View(kelasehnameh);
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var kelasehnameh = await _context.GetKelasehnamehByIdAsync(id, userId);

            if (kelasehnameh == null)
            {
                return NotFound();
            }

            return View(kelasehnameh);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodeKelaseh,NameKelaseh,UserId")] Kelasehnameh kelasehnameh)
        {
            var userId = GetCurrentUserId();

            if (id != kelasehnameh.Id || kelasehnameh.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.UpdateKelasehnamehAsync(kelasehnameh);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await KelasehnamehExistsAsync(kelasehnameh.Id, userId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(kelasehnameh);
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var kelasehnameh = await _context.GetKelasehnamehByIdAsync(id, userId);

            if (kelasehnameh == null)
            {
                return NotFound();
            }

            return View(kelasehnameh);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var kelasehnameh = await _context.GetKelasehnamehByIdAsync(id, userId);
            var letters = await _letter.GetAllLettersAsync();
            var filter = (List<Letter>)letters.Data!;
            var lis = filter.Where(x => x.FileCode == kelasehnameh.CodeKelaseh.ToString());

            var klsmain = await _context.GetKelasehnamehByIdAsync(1, userId);

            if (kelasehnameh.Id == 1)
            {
                TempData["Error"] = "شما قادر به حذف کلاسه اصلی نمی باشید";
                return RedirectToAction(nameof(Index));
            }

            if (lis != null && klsmain != null)
            {
                foreach (var item in lis)
                {
                    item.FileCode = klsmain.CodeKelaseh.ToString();
                    item.FileDescription = klsmain.NameKelaseh;
                    _dbcon.Letters.Update(item);
                    await _dbcon.SaveChangesAsync();
                }
            }

            await _context.DeleteKelasehnamehAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> KelasehnamehExistsAsync(int id, string userId)
        {
            var klss = await _context.GetAllKelasehnamehsAsync(userId);
            return klss.Any(e => e.Id == id);
        }
    }
}