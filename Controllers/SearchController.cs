using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public SearchController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // دالة البحث والفرز الشاملة
        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, string? searchQuery)
        {
            // 1. جلب قائمة التخصصات لإرسالها إلى القائمة المنسدلة في شاشة البحث
            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", categoryId);

            ViewBag.SearchQuery = searchQuery; // للاحتفاظ بالكلمة المكتوبة في مربع البحث

            // 2. استعلام مبدئي لجلب الحرفيين النشطين مع تخصصاتهم
            var query = _userManager.Users
                .Include(u => u.Category)
                .Where(u => u.Role == UserRole.Worker && u.IsActive);

            // 3. الفرز حسب "نوع العمل" (إذا اختار العميل تخصصاً من القائمة)
            if (categoryId.HasValue)
            {
                query = query.Where(u => u.CategoryId == categoryId.Value);
            }

            // 4. الفرز حسب "الوصف والاسم" (إذا كتب العميل كلمة مفتاحية)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(u =>
                    u.FullName.Contains(searchQuery) ||
                    (u.Bio != null && u.Bio.Contains(searchQuery)));
            }

            // 5. ترتيب النتائج حسب الحرفي الأعلى تقييماً أولاً
            var workers = await query.OrderByDescending(u => u.Rating).ToListAsync();

            return View(workers);
        }
    }
}