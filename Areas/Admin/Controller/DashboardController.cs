using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Areas.Admin.Controllers
{
    // 🔴 ربط الكنترولر بمنطقة الإدارة
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. حماية صارمة: التأكد أن المستخدم الحالي هو "مدير" فعلاً
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Role != UserRole.Admin)
            {
                TempData["ErrorMessage"] = "غير مصرح لك بالدخول إلى لوحة التحكم.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // 2. جلب الإحصائيات من قاعدة البيانات وإرسالها للـ ViewBag
            ViewBag.TotalClients = await _context.Users.CountAsync(u => u.Role == UserRole.Client);
            ViewBag.TotalWorkers = await _context.Users.CountAsync(u => u.Role == UserRole.Worker);
            ViewBag.TotalCategories = await _context.Categories.CountAsync(); // يمكن إضافة (c => c.IsActive) لحساب المفعلة فقط
            ViewBag.TotalTasks = await _context.Tasks.CountAsync();

            // 3. جلب أحدث 5 طلبات في المنصة لعرضها في الجدول السفلي
            var recentTasks = await _context.Tasks
                .Include(t => t.Client)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            // 4. إرسال الطلبات الحديثة كـ Model للشاشة
            return View(recentTasks);
        }
    }
}