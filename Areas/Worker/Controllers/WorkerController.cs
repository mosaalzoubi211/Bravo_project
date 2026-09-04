using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Controllers
{
    [Area("Worker")]
    [Authorize]
    public class WorkerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public WorkerController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

// 1. عرض الطلبات المتاحة في السوق (سوق العمل)
        [HttpGet]
        public async Task<IActionResult> Index() // تم تغيير الاسم إلى Index ليتطابق مع النافبار
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            // حماية إضافية: التأكد من أن المستخدم حرفي فعلاً
            if (user == null || user.Role != UserRole.Worker)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // جلب الطلبات العامة (ReceivingOffers) والطلبات المباشرة (Pending)
            var availableTasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Where(t => t.Status == Bravo.Models.TaskStatus.Pending || t.Status == Bravo.Models.TaskStatus.ReceivingOffers)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(availableTasks); // تأكد من أن اسم شاشة العرض في المجلد هو Index.cshtml
        }

        // 2. معالجة قبول الطلب من قبل الحرفي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTask(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // البحث عن الطلب في قاعدة البيانات
            var task = await _context.Tasks.FindAsync(id);

            // التأكد من أن الطلب موجود ولم يسبق لأحد قبوله
            if (task == null || task.Status != Bravo.Models.TaskStatus.Pending)
            {
                TempData["ErrorMessage"] = "عذراً، هذا الطلب لم يعد متاحاً أو تم قبوله من حرفي آخر.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);

            // تحديث الطلب: ربطه بالحرفي الحالي وتغيير حالته
            task.WorkerId = userId;
            task.Status = Bravo.Models.TaskStatus.Accepted;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم قبول الطلب بنجاح! يمكنك الآن رؤيته في قائمة مهامك.";
            return RedirectToAction(nameof(MyJobs));
        }

        // 3. عرض المهام التي قام الحرفي بقبولها لكي ينفذها
        [HttpGet]
        public async Task<IActionResult> MyJobs()
        {
            var userId = _userManager.GetUserId(User);

            // جلب المهام الخاصة بهذا الحرفي تحديداً
            var myJobs = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Where(t => t.WorkerId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(myJobs);
        }

        // 4. تحديث حالة الطلب إلى مكتمل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // جلب الطلب والتأكد من أنه يخص هذا الحرفي تحديداً
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.WorkerId == userId);

            // التأكد من أن الطلب موجود وحالته الحالية "تم القبول"
            if (task == null || task.Status != Bravo.Models.TaskStatus.Accepted)
            {
                TempData["ErrorMessage"] = "عذراً، لا يمكن تحديث حالة هذا الطلب.";
                return RedirectToAction(nameof(MyJobs));
            }

            // تحديث حالة الطلب إلى مكتمل
            task.Status = Bravo.Models.TaskStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "عمل رائع! تم تسجيل المهمة كمكتملة بنجاح.";
            return RedirectToAction(nameof(MyJobs));
        }
        [HttpGet]
        public async Task<IActionResult> DirectRequests()
        {
            var userId = _userManager.GetUserId(User);

            // جلب الطلبات التي حالتها Pending و موجهة خصيصاً لهذا الحرفي
            var directTasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Where(t => t.WorkerId == userId && t.Status == Bravo.Models.TaskStatus.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(directTasks);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectRequests(string taskId, decimal price)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.WorkerId == userId);

            if (task == null || task.Status != Bravo.Models.TaskStatus.Pending)
            {
                TempData["ErrorMessage"] = "عذراً، هذا الطلب غير متاح.";
                return RedirectToAction(nameof(DirectRequests));
            }

            if (price <= 0)
            {
                TempData["ErrorMessage"] = "يجب إدخال سعر صحيح للموافقة على الطلب.";
                return RedirectToAction(nameof(DirectRequests));
            }

            // تحديث حالة الطلب وإضافة السعر الذي حدده الحرفي
            task.Status = Bravo.Models.TaskStatus.Accepted;
            task.AgreedPrice = price;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم قبول الطلب بنجاح وتحديد السعر بقيمة {price} دينار.";
            return RedirectToAction(nameof(MyJobs));
        }

        // 7. الاعتذار عن الطلب المباشر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineDirectRequest(string taskId)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.WorkerId == userId);

            if (task != null && task.Status == Bravo.Models.TaskStatus.Pending)
            {
                // تحويل الطلب إلى "ملغي" ليعرف العميل أن الحرفي اعتذر
                task.Status = Bravo.Models.TaskStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم الاعتذار عن الطلب بنجاح.";
            }

            return RedirectToAction(nameof(DirectRequests));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptDirectRequest(string taskId, decimal price)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.WorkerId == userId);

            if (task == null || task.Status != Bravo.Models.TaskStatus.Pending)
            {
                TempData["ErrorMessage"] = "عذراً، هذا الطلب غير متاح.";
                return RedirectToAction(nameof(DirectRequests)); // البقاء في الوارد في حال الخطأ
            }

            // تحديث حالة الطلب وإضافة السعر
            task.Status = Bravo.Models.TaskStatus.Accepted;
            task.AgreedPrice = price;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم قبول الطلب بنجاح وتحديد السعر بقيمة {price} دينار.";

            // هذا هو السطر المسؤول عن نقلك لشاشة مهامي فوراً
            return RedirectToAction(nameof(MyJobs));
        }
        // 8. عرض شاشة الملف الشخصي للحرفي
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            var model = new Bravo.ViewModels.WorkerProfileViewModel
            {
                CategoryId = user.CategoryId ?? 0,
                Bio = user.Bio ?? string.Empty
            };

            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Bravo.ViewModels.WorkerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user != null)
            {
                user.CategoryId = model.CategoryId;
                user.Bio = model.Bio;

                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = "تم تحديث ملفك الشخصي بنجاح! أنت الآن تظهر للعملاء في نتائج البحث.";
            }

            return RedirectToAction(nameof(Profile));
        }
        // دالة تقديم عرض سعر على طلب عام
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOffer(string taskId, decimal proposedPrice, string workerMessage)
        {
            var userId = _userManager.GetUserId(User);

            // 1. فحص هل الـ ID يصل من الشاشة أم لا
            if (string.IsNullOrEmpty(taskId))
            {
                TempData["ErrorMessage"] = "خطأ تقني: لم يتم التعرف على رقم الطلب (ID).";
                return RedirectToAction(nameof(Index));
            }

            // 2. البحث عن الطلب بغض النظر عن حالته
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                TempData["ErrorMessage"] = "عذراً، هذا الطلب غير موجود في قاعدة البيانات.";
                return RedirectToAction(nameof(Index));
            }


            if (task.Status != Bravo.Models.TaskStatus.ReceivingOffers)
            {
                TempData["ErrorMessage"] = $"عذراً، حالة هذا الطلب الحالية هي ({task.Status})، ولكي يقبل عروضاً يجب أن تكون حالته (ReceivingOffers).";
                return RedirectToAction(nameof(Index));
            }

            // 4. التحقق من عدم تكرار العرض
            var existingOffer = await _context.TaskOffers.FirstOrDefaultAsync(o => o.TaskId == taskId && o.WorkerId == userId);
            if (existingOffer != null)
            {
                TempData["ErrorMessage"] = "لقد قمت بتقديم عرض على هذا الطلب مسبقاً.";
                return RedirectToAction(nameof(Index));
            }

            // 5. التحقق من السعر
            if (proposedPrice <= 0)
            {
                TempData["ErrorMessage"] = "يرجى إدخال سعر صحيح أكبر من الصفر.";
                return RedirectToAction(nameof(Index));
            }

            // إنشاء وحفظ العرض
            var offer = new TaskOffer
            {
                TaskId = taskId,
                WorkerId = userId!,
                ProposedPrice = proposedPrice,
                WorkerMessage = workerMessage ?? "بدون رسالة"
            };

            _context.TaskOffers.Add(offer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تقديم عرضك بنجاح! سيقوم العميل بمراجعته.";
            return RedirectToAction(nameof(Index));
        }
    }
}
