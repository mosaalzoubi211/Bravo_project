using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Bravo.Data;
using Bravo.Models;
using Bravo.ViewModels;

namespace Bravo.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Bravo.Models.AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<Bravo.Models.AppUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            // جلب اسم التخصص يدوياً وإرساله للشاشة
            if (user.Role == UserRole.Worker && user.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(user.CategoryId.Value);
                ViewBag.CategoryName = category?.Name ?? "غير محدد";
            }

            return View(user);
        }

        // 2. شاشة التعديل
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsWorker = user.Role == UserRole.Worker,
                CategoryId = user.CategoryId,
                Bio = user.Bio
            };

            if (model.IsWorker)
            {
                ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
            }

            return View(model);
        }

        // 3. استلام التعديلات ونقلها لصفحة كلمة المرور
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // حفظ التعديلات مؤقتاً بصيغة JSON والانتقال لصفحة التأكيد
            TempData["PendingChanges"] = JsonSerializer.Serialize(model);
            return RedirectToAction(nameof(ConfirmPassword));
        }

        // 4. شاشة تأكيد كلمة المرور
        [HttpGet]
        public IActionResult ConfirmPassword()
        {
            if (!TempData.ContainsKey("PendingChanges"))
            {
                return RedirectToAction(nameof(Index));
            }
            // الاحتفاظ بالبيانات المؤقتة لكي لا تضيع
            TempData.Keep("PendingChanges");
            return View();
        }

        // 5. التحقق من كلمة المرور وحفظ البيانات نهائياً
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPassword(string password)
        {
            TempData.Keep("PendingChanges"); // إبقاء البيانات في حال أخطأ بالرقم السري

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "يرجى إدخال كلمة المرور.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // فحص كلمة المرور
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "كلمة المرور غير صحيحة.");
                return View();
            }

            // استرجاع البيانات المؤقتة
            var pendingData = TempData["PendingChanges"]?.ToString();
            if (string.IsNullOrEmpty(pendingData)) return RedirectToAction(nameof(Index));

            var model = JsonSerializer.Deserialize<UserProfileViewModel>(pendingData);
            if (model == null) return RedirectToAction(nameof(Index));

            // حفظ التعديلات نهائياً
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            if (user.Role == UserRole.Worker)
            {
                user.CategoryId = model.CategoryId;
                user.Bio = model.Bio;
            }

            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "تم تحديث بياناتك بنجاح وبأمان.";
            return RedirectToAction(nameof(Index));
        }
    }
}