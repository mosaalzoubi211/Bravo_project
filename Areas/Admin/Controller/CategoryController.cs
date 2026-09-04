using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public CategoryController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // دالة مساعدة للتحقق من صلاحية المدير
        private async Task<bool> IsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null && user.Role == UserRole.Admin;
        }

        // 1. عرض جميع التخصصات
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin()) return RedirectToAction("Index", "Home", new { area = "" });

            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // 2. شاشة إضافة تخصص جديد
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!await IsAdmin()) return RedirectToAction("Index", "Home", new { area = "" });
            return View();
        }

        // 3. حفظ التخصص الجديد
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!await IsAdmin()) return RedirectToAction("Index", "Home", new { area = "" });

            if (ModelState.IsValid)
            {
                category.IsActive = true; // مفعل افتراضياً
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تمت إضافة التخصص بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 4. تفعيل / إيقاف تخصص (بدون حذفه للحفاظ على بيانات الحرفيين)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (!await IsAdmin()) return RedirectToAction("Index", "Home", new { area = "" });

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive; // عكس الحالة الحالية
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = category.IsActive ? "تم تفعيل التخصص بنجاح." : "تم إيقاف التخصص بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}