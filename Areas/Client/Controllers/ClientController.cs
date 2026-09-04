using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bravo.Data;
using Bravo.Models;
using Bravo.ViewModels;

namespace Bravo.Controllers
{
    [Area("Client")]
    [Authorize]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;
        // 1. حقن أداة البيئة للوصول إلى مجلدات الخادم (wwwroot)
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ClientController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult CreateTask()
        {
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name");
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var task = new ServiceTask
            {
                ClientId = userId!,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Location = model.Location,
                ScheduledDate = model.ScheduledDate,
                PaymentMethod = model.PaymentMethod,
                Status = Bravo.Models.TaskStatus.ReceivingOffers,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(); // نحفظ الطلب أولاً لكي نحصل على الـ Id الخاص به

            // 2. منطق معالجة ورفع الصورة (إن وجدت)
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // تحديد مسار مجلد الحفظ (wwwroot/uploads/tasks)
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tasks");

                // إذا لم يكن المجلد موجوداً، قم بإنشائه تلقائياً
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // إنشاء اسم فريد للصورة لمنع تداخل الأسماء (باستخدام Guid)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // حفظ الملف فعلياً على الخادم
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                // حفظ مسار الصورة في قاعدة البيانات
                var taskMedia = new TaskMedia
                {
                    TaskId = task.Id,
                    FileUrl = "/uploads/tasks/" + uniqueFileName, // المسار النسبي لعرضها في المتصفح
                    FileType = model.ImageFile.ContentType
                };

                _context.TaskMedia.Add(taskMedia);
                await _context.SaveChangesAsync(); // حفظ الصورة في جدول TaskMedia
            }

            TempData["SuccessMessage"] = "تم نشر طلبك في السوق بنجاح! بانتظار عروض الحرفيين.";
            return RedirectToAction(nameof(MyTasks));
        }

        [HttpGet]
        public async Task<IActionResult> MyTasks()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.ClientId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tasks);
        }
        // 4. عرض تفاصيل الطلب (بما فيها الصور)
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // جلب الطلب مع التخصص، الحرفي (إذا تم القبول)، والصور المرفقة
            var task = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Worker)
                .Include(t => t.MediaFiles)
                .FirstOrDefaultAsync(t => t.Id == id && t.ClientId == userId);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }
        // 5. تقييم الحرفي بعد انتهاء المهمة
        // 10. شاشة التقييم (تظهر بعد اكتمال المهمة)
        [HttpGet]
        public async Task<IActionResult> RateWorker(string taskId)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.Tasks
                .Include(t => t.Worker)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ClientId == userId);

            if (task == null || task.Status != Bravo.Models.TaskStatus.Completed)
            {
                TempData["ErrorMessage"] = "عذراً، لا يمكن تقييم هذه المهمة إلا بعد أن يقوم الحرفي بإنهائها.";
                return RedirectToAction(nameof(MyTasks));
            }

            if (task.ClientRating.HasValue)
            {
                TempData["ErrorMessage"] = "لقد قمت بتقييم هذا الحرفي على هذه المهمة مسبقاً.";
                return RedirectToAction(nameof(MyTasks));
            }

            return View(task);
        }

        // 11. حفظ التقييم وتحديث متوسط تقييم الحرفي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateWorker(string taskId, int rating, string review)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.Tasks
                .Include(t => t.Worker)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ClientId == userId);

            if (task == null || task.Status != Bravo.Models.TaskStatus.Completed) return NotFound();

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "يرجى اختيار تقييم من 1 إلى 5 نجوم.";
                return RedirectToAction(nameof(RateWorker), new { taskId = taskId });
            }

            // 1. حفظ تقييم الطلب
            task.ClientRating = rating;
            task.ClientReview = review;

            // 2. تحديث التقييم العام للحرفي
            var worker = task.Worker;
            if (worker != null)
            {
                // جلب كل التقييمات السابقة لهذا الحرفي
                var allRatings = await _context.Tasks
                    .Where(t => t.WorkerId == worker.Id && t.ClientRating.HasValue)
                    .Select(t => t.ClientRating.Value)
                    .ToListAsync();

                // إضافة التقييم الحالي وحساب المتوسط الجديد
                allRatings.Add(rating);
                worker.Rating = allRatings.Average();
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "شكراً لك! تم تقييم الحرفي بنجاح، تقييمك سيساعد العملاء الآخرين.";
            return RedirectToAction(nameof(MyTasks));
        }
        [HttpGet]
        public async Task<IActionResult> SearchWorkers(string? searchQuery)
        {
            // نبدأ بجلب جميع المستخدمين الذين دورهم "حرفي" وحسابهم مفعل
            var workersQuery = _userManager.Users.Where(u => u.Role == UserRole.Worker && u.IsActive);

            // إذا قام العميل بكتابة كلمة في البحث، نفلتر النتائج
            if (!string.IsNullOrEmpty(searchQuery))
            {
                workersQuery = workersQuery.Where(u =>
                    u.FullName.Contains(searchQuery) ||
                    (u.Bio != null && u.Bio.Contains(searchQuery)));
            }

            // ترتيب الحرفيين حسب التقييم الأعلى أولاً
            var workers = await workersQuery.OrderByDescending(u => u.Rating).ToListAsync();

            ViewBag.SearchQuery = searchQuery; // للاحتفاظ بكلمة البحث في مربع النص

            return View(workers);

        }
        // شاشة الطلب المباشر لحرفي معين
        [HttpGet]
        public async Task<IActionResult> DirectRequest(string workerId)
        {
            if (string.IsNullOrEmpty(workerId)) return NotFound();

            // جلب بيانات الحرفي لعرضها في الشاشة
            var worker = await _userManager.Users.Include(u => u.Category).FirstOrDefaultAsync(u => u.Id == workerId);
            if (worker == null || worker.Role != UserRole.Worker) return NotFound();

            ViewBag.Worker = worker;

            var model = new CreateTaskViewModel
            {
                // تثبيت التخصص بناءً على تخصص الحرفي
                CategoryId = worker.CategoryId ?? 0
            };

            return View(model);
        }

        // معالجة وحفظ الطلب المباشر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectRequest(CreateTaskViewModel model, string workerId)
        {
            // 1. التحقق من وصول هوية الحرفي
            if (string.IsNullOrEmpty(workerId))
            {
                TempData["ErrorMessage"] = "حدث خطأ: لم يتم التعرف على الحرفي المطلوب.";
                return RedirectToAction("Index", "Search");
            }

            // 2. معالجة مشكلة "التخصص المفقود" (CategoryId = 0)
            if (model.CategoryId == 0)
            {
                // إذا كان الحرفي لا يمتلك تخصصاً بعد، نسند الطلب لأول تخصص موجود في الداتا بيس لمنع الخطأ
                var defaultCategory = await _context.Categories.FirstOrDefaultAsync();
                if (defaultCategory != null)
                {
                    model.CategoryId = defaultCategory.Id;
                }
                else
                {
                    TempData["ErrorMessage"] = "حدث خطأ: لا يوجد تخصصات متاحة في النظام.";
                    return RedirectToAction("Index", "Search");
                }
            }


            if (!ModelState.IsValid)
            {

                var worker = await _userManager.FindByIdAsync(workerId);
                ViewBag.Worker = worker;
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var task = new ServiceTask
            {
                ClientId = userId!,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Location = model.Location,
                ScheduledDate = model.ScheduledDate,
                PaymentMethod = model.PaymentMethod,


                Status = Bravo.Models.TaskStatus.ReceivingOffers,

                CreatedAt = DateTimeOffset.UtcNow
            };

            // 4. استخدام Try-Catch لاصطياد أي خطأ في قاعدة البيانات دون توقف البرنامج
            try
            {
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // إذا حدث خطأ غير متوقع، نعيد العميل للشاشة مع رسالة توضيحية
                TempData["ErrorMessage"] = "عذراً، حدث خطأ أثناء حفظ الطلب في قاعدة البيانات.";
                var worker = await _userManager.FindByIdAsync(workerId);
                ViewBag.Worker = worker;
                return View(model);
            }

            TempData["SuccessMessage"] = "تم إرسال طلبك المباشر للحرفي بنجاح! بانتظار موافقته.";
            return RedirectToAction(nameof(MyTasks));
        }
        // 8. عرض العروض المقدمة لطلب معين
        [HttpGet]
        public async Task<IActionResult> TaskOffers(string taskId)
        {
            var userId = _userManager.GetUserId(User);

            // جلب الطلب مع جميع العروض وبيانات الحرفيين الذين قدموها
            var task = await _context.Tasks
                .Include(t => t.Offers)
                    .ThenInclude(o => o.Worker)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ClientId == userId);

            if (task == null)
            {
                TempData["ErrorMessage"] = "الطلب غير موجود أو لا تملك صلاحية الوصول إليه.";
                return RedirectToAction(nameof(MyTasks));
            }

            return View(task);
        }

        // 9. قبول عرض سعر معين
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer(string offerId)
        {
            var userId = _userManager.GetUserId(User);

            // جلب العرض مع الطلب المرتبط به للتأكد من الملكية
            var offer = await _context.TaskOffers
                .Include(o => o.Task)
                .FirstOrDefaultAsync(o => o.Id == offerId && o.Task!.ClientId == userId);

            if (offer == null || offer.Task == null || offer.Task.Status != Bravo.Models.TaskStatus.ReceivingOffers)
            {
                TempData["ErrorMessage"] = "عذراً، هذا العرض غير متاح أو تم إغلاق الطلب.";
                return RedirectToAction(nameof(MyTasks));
            }

            // سحر النظام المزدوج: نقوم بتحديث الطلب بناءً على العرض الفائز
            offer.Task.Status = Bravo.Models.TaskStatus.Accepted; // تغيير الحالة لقيد التنفيذ
            offer.Task.WorkerId = offer.WorkerId;                 // إسناد الطلب للحرفي الفائز
            offer.Task.AgreedPrice = offer.ProposedPrice;         // اعتماد السعر المتفق عليه
            offer.IsAccepted = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم قبول عرض الحرفي {offer.Worker?.FullName} بقيمة {offer.ProposedPrice} دينار بنجاح! الطلب الآن قيد التنفيذ.";
            return RedirectToAction(nameof(MyTasks));
        }
        // 12. إلغاء الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTask(string taskId)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ClientId == userId);

            if (task == null) return NotFound();

            // السماح بالإلغاء فقط إذا لم يتم البدء بالتنفيذ
            if (task.Status == Bravo.Models.TaskStatus.Completed || task.Status == Bravo.Models.TaskStatus.Accepted)
            {
                TempData["ErrorMessage"] = "عذراً، لا يمكن إلغاء الطلب بعد قبوله أو اكتماله.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            if (task.Status != Bravo.Models.TaskStatus.Cancelled)
            {
                task.Status = Bravo.Models.TaskStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إلغاء الطلب بنجاح.";
            }

            return RedirectToAction(nameof(MyTasks));
        }
    }
}