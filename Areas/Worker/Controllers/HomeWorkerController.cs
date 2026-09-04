using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bravo.Data;
using Bravo.Models;

namespace Bravo.Areas.Worker.Controllers
{
    [Area("Worker")]
    [Authorize]
    public class HomeWorkerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Bravo.Models.AppUser> _userManager;

        public HomeWorkerController(ApplicationDbContext context, UserManager<Bravo.Models.AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // حماية أمنية للحرفيين فقط
            if (user == null || user.Role != UserRole.Worker)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var userId = user.Id;

            // 1. حساب الإحصائيات الخاصة بهذا الحرفي وعرضها عبر ViewBag
            ViewBag.CompletedTasks = await _context.Tasks
                .CountAsync(t => t.WorkerId == userId && t.Status == Bravo.Models.TaskStatus.Completed);

            ViewBag.ActiveTasks = await _context.Tasks
                .CountAsync(t => t.WorkerId == userId && t.Status == Bravo.Models.TaskStatus.Accepted);

            ViewBag.CancelledTasks = await _context.Tasks
                .CountAsync(t => t.WorkerId == userId && t.Status == Bravo.Models.TaskStatus.Cancelled);

            // 2. جلب الطلبات المتاحة في السوق (بانتظار حرفي)
            var availableTasks = await _context.Tasks
                .Include(t => t.Category) // جلب اسم التخصص
                .Include(t => t.Client)
                .Where(t => t.Status == Bravo.Models.TaskStatus.Pending || t.Status == Bravo.Models.TaskStatus.ReceivingOffers)
                .OrderByDescending(t => t.CreatedAt)
                .Take(12) // جلب أحدث 12 طلب
                .ToListAsync();

            return View(availableTasks);
        }
    }
}