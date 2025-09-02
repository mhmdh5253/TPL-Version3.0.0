using BE;
using BLL.Tokening;
using BLL.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("TokenManagement")]
    public class TokenController : Controller
    {
        private readonly BlToken _tokenRepository;
        private readonly ILogger<TokenController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BlWallet _wallet;
        public TokenController(BlToken tokenRepository, ILogger<TokenController> logger, UserManager<ApplicationUser> userManager, BlWallet wallet)
        {
            _tokenRepository = tokenRepository;
            _logger = logger;
            _userManager = userManager;
            _wallet = wallet;
        }

        // GET: Token
        [HttpGet("Tokens")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = _userManager.Users.FirstOrDefault(x => x.UserName == User.Identity!.Name)!;
                var balance = (int)await _wallet.GetBalanceAsync(user.Id);
                if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Ostan) || string.IsNullOrEmpty(user.Email) || user.PhoneNumberConfirmed == false || balance <= 10000)
                {

                    TempData["email"] = "خطا : 1.موجودی کیف پول نمی تواند کمتر از 10 هزارتومان باشد \n 2.اطلاعات پروفایل تکمیل نیست.";
                    return RedirectToAction("ProfilePage", "Account");
                }
                var tokens = await _tokenRepository.GetUserTokensAsync(user.Id);
                return View(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tokens list");
                return View("Error");
            }
        }

        // GET: Token/Create
        [HttpGet("CreateNewToken")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Token/Create
        [HttpPost("CreateNewToken")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TokenGenerationDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = await _tokenRepository.CreateTokenAsync(
                        dto.UserId!, dto.Description!,
                        DateTime.UtcNow.AddDays(dto.ExpiresInDays));

                    TempData["SuccessMessage"] = "توکن با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Details), new { id = token.Id });
                }
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token");
                ModelState.AddModelError("", "خطا در ایجاد توکن");
                return View(dto);
            }
        }

        // GET: Token/Details/5
        [HttpGet("TokenDetail/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var token = await _tokenRepository.GetTokenByIdAsync(id);
                if (token == null)
                {
                    return NotFound();
                }

                // بررسی دسترسی کاربر به توکن
                if (!User.IsInRole("Admin") && token.UserId != User.Identity!.Name)
                {
                    return Forbid();
                }

                return View(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting token details for id {id}");
                return View("Error");
            }
        }

        // POST: Token/Revoke/5
        [HttpPost("RevokeToken")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(int id)
        {
            try
            {
                await _tokenRepository.RevokeTokenAsync(id);
                TempData["SuccessMessage"] = "توکن با موفقیت ابطال شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking token with id {id}");
                TempData["ErrorMessage"] = "خطا در ابطال توکن";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Token/Validate
        [HttpGet("ValidateToken")]
        public IActionResult Validate()
        {
            return View();
        }

        // POST: Token/Validate
        [HttpPost("ValidateToken")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validate(string tokenValue)
        {
            try
            {
                var isValid = await _tokenRepository.ValidateTokenAsync(tokenValue);
                ViewBag.ValidationResult = isValid ? "توکن معتبر است" : "توکن نامعتبر است";
                ViewBag.IsValid = isValid;

                if (isValid)
                {
                    var token = await _tokenRepository.GetTokenByValueAsync(tokenValue);
                    return View("ValidationResult", token);
                }

                return View("ValidationResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                ModelState.AddModelError("", "خطا در اعتبارسنجی توکن");
                return View();
            }
        }
    }

    public class TokenGenerationDto
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "مدت اعتبار الزامی است")]
        [Range(1, 365, ErrorMessage = "مدت اعتبار باید بین 1 تا 365 روز باشد")]
        public int ExpiresInDays { get; set; } = 30;
        [Display(Name = "توضیحات")]
        [MaxLength(250, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? Description { get; set; }
    }
}