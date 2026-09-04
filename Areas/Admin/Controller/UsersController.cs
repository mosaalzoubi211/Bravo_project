using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bravo.Models;

namespace Bravo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // دالة للتحقق من صلاحية المدير
        private async Task<bool> IsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            return user != null && user.Role == UserRole.Admin;
        }

        // 1. عرض قائمة المستخدمين (مع فلترة حسب الدور)
        [HttpGet]
        public async Task<IActionResult> Index(UserRole? role)
        {
            if (!await IsAdmin()) return RedirectToAction("Index", "Home", new { area = "" });

            var query = _userManager.Users.AsQueryable();

            // تطبيق الفلتر إذا ضغط المدير على (عملاء فقط) أو (حرفيين فقط)
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            // إخفاء حسابات المدراء من القائمة لحمايتها من الإيقاف بالخطأ
            query = query.Where(u => u.Role != UserRole.Admin);

            var users = await query.ToListAsync();

            ViewBag.SelectedRole = role; // للحفاظ على الزر نشطاً في الواجهة
            return View(users);
        }

        // 2. إيقاف / تفعيل حساب المستخدم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (!await IsAdmin()) return RedirectToAction("Index", "Home", new { area = "" });

            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null) return NotFound();

            // تفعيل نظام الحظر لهذا الحساب إذا لم يكن مفعلاً
            await _userManager.SetLockoutEnabledAsync(targetUser, true);

            // التحقق هل الحساب محظور حالياً؟
            bool isBanned = targetUser.LockoutEnd != null && targetUser.LockoutEnd > DateTimeOffset.UtcNow;

            if (isBanned)
            {
                // فك الحظر
                await _userManager.SetLockoutEndDateAsync(targetUser, null);
                TempData["SuccessMessage"] = $"تم تفعيل حساب ({targetUser.FullName}) بنجاح.";
            }
            else
            {
                // حظر الحساب لمدة 100 سنة (حظر نهائي)
                await _userManager.SetLockoutEndDateAsync(targetUser, DateTimeOffset.MaxValue);
                TempData["SuccessMessage"] = $"تم إيقاف حساب ({targetUser.FullName}) بنجاح.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}