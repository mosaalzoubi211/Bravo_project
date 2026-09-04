using Bravo.Models;
using Bravo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bravo.Areas.Auth.Controllers
{
    [Area("Auth")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 1. عرض صفحة التسجيل
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 2. معالجة بيانات التسجيل
        [HttpPost]
        [ValidateAntiForgeryToken] // حماية ضد ثغرات CSRF
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model); // إعادة المستخدم لنفس الصفحة مع عرض الأخطاء

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني مسجل بالفعل.");
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.Phone,
                Role = model.Role,
                IsActive = true,
                IsAvailable = model.Role == UserRole.Worker
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // 3. عرض صفحة تسجيل الدخول
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
                return View(model);
            }

            // استخدام إعداد RememberMe لإنشاء Cookie دائم أو مؤقت
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // توجيه المستخدم للصفحة التي طلبها قبل تسجيل الدخول (إن وجدت)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // التوجيه الذكي حسب نوع الحساب (المدير، الحرفي، العميل)
                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (user.Role == UserRole.Worker)
                {
                    return RedirectToAction("Index", "HomeWorker", new { area = "Worker" }); // بافتراض أن اسم الكنترولر Worker
                }
                else if (user.Role == UserRole.Client)
                {
                    return RedirectToAction("Index", "HomeClient", new { area = "Client" }); // بافتراض أن اسم الكنترولر Client
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }

            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            return View(model);
        }

        // 5. تسجيل الخروج
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
