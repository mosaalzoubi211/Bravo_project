using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Controllers
{
    [Authorize] // تأكيد أن المستخدم مسجل دخول
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. عرض شاشة المحادثة الخاصة بطلب معين
        [HttpGet]
        public async Task<IActionResult> Index(string taskId)
        {
            if (string.IsNullOrEmpty(taskId)) return NotFound();

            var userId = _userManager.GetUserId(User);

            // جلب الطلب مع بيانات العميل والحرفي للتأكد من الصلاحيات
            var task = await _context.Tasks
                .Include(t => t.Client)
                .Include(t => t.Worker)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return NotFound();

            // حماية أمنية: يمنع دخول أي شخص للمحادثة باستثناء صاحب الطلب أو الحرفي المنفذ له
            if (task.ClientId != userId && task.WorkerId != userId)
            {
                TempData["ErrorMessage"] = "غير مصرح لك بدخول هذه المحادثة.";
                return RedirectToAction("Index", "Home");
            }

            // جلب الرسائل السابقة وترتيبها حسب وقت الإرسال
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.TaskId == taskId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // تحويل الرسائل الواردة للمستخدم الحالي إلى "مقروءة"
            var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            // تحديد "الطرف الآخر" بذكاء (إذا كنت أنت العميل فالطرف الآخر هو الحرفي، والعكس)
            var receiverId = (userId == task.ClientId) ? task.WorkerId : task.ClientId;
            var receiverName = (userId == task.ClientId) ? task.Worker?.FullName : task.Client?.FullName;

            // إرسال البيانات الأساسية للشاشة عبر ViewBag
            ViewBag.TaskId = taskId;
            ViewBag.ReceiverId = receiverId;
            ViewBag.ReceiverName = receiverName ?? "المستخدم الآخر";
            ViewBag.CurrentUserId = userId;
            ViewBag.TaskDescription = task.Description;
            ViewBag.TaskStatus = task.Status; // لإيقاف المحادثة إذا انتهى الطلب

            return View(messages);
        }

        // 2. إرسال رسالة جديدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string taskId, string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Index), new { taskId = taskId });
            }

            var userId = _userManager.GetUserId(User);

            var message = new ChatMessage
            {
                TaskId = taskId,
                SenderId = userId!,
                ReceiverId = receiverId, // الطرف الآخر
                Content = content,
                SentAt = DateTimeOffset.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // إعادة التوجيه لنفس المحادثة لرؤية الرسالة الجديدة
            return RedirectToAction(nameof(Index), new { taskId = taskId });
        }
    }
}