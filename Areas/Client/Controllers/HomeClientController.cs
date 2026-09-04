using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class HomeClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public HomeClientController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. جلب بيانات العميل الذي قام بتسجيل الدخول حالياً
            var user = await _userManager.GetUserAsync(User);

            // حماية أمنية: التأكد أن من يحاول الدخول هو عميل فعلاً
            if (user == null || user.Role != UserRole.Client)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // 2. جلب الطلبات المفتوحة الخاصة بهذا العميل تحديداً
            // (تأكد من اسم خاصية معرّف العميل في كلاس ServiceTask، غالباً تكون ClientId)
            var myTasks = await _context.Tasks
                .Where(t => t.ClientId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // 3. تمرير قائمة الطلبات إلى الشاشة
            return View(myTasks);
        }
    }
}