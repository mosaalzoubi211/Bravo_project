using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Bravo.Models;

namespace Bravo.Data
{
    // 1. الوراثة من IdentityDbContext أساسية لعمل نظام تسجيل الدخول
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // الجداول (لا حاجة لتعريف Users لأنه موجود مسبقاً داخل IdentityDbContext)
        public DbSet<Category> Categories { get; set; }
        public DbSet<ServiceTask> Tasks { get; set; }
        public DbSet<TaskMedia> TaskMedia { get; set; }
        public DbSet<TaskOffer> TaskOffers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // هذا السطر يجب أن يكون في البداية دائماً لتهيئة جداول الصلاحيات
            base.OnModelCreating(modelBuilder);

            // إعداد علاقات الرسائل (منع الحذف المتسلسل للمستخدمين)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // حذف الرسائل إذا تم حذف الطلب نفسه
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Task)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            // --- الكود الخاص بك (الضروري جداً لحماية قاعدة البيانات) ---

            // 1. إعداد علاقة العميل بالطلب (منع الحذف التلقائي المتسلسل)
            modelBuilder.Entity<ServiceTask>()
                .HasOne(t => t.Client)
                .WithMany(u => u.ClientTasks)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. إعداد علاقة الحرفي بالطلب (منع الحذف التلقائي المتسلسل)
            modelBuilder.Entity<ServiceTask>()
                .HasOne(t => t.Worker)
                .WithMany(u => u.WorkerTasks)
                .HasForeignKey(t => t.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. إعداد علاقة الطلب بالصور المرفقة (السماح بالحذف التلقائي)
            modelBuilder.Entity<TaskMedia>()
                .HasOne(m => m.Task)
                .WithMany(t => t.MediaFiles)
                .HasForeignKey(m => m.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // إعداد علاقة العرض بالحرفي (منع الحذف المتسلسل)
            modelBuilder.Entity<TaskOffer>()
                .HasOne(o => o.Worker)
                .WithMany()
                .HasForeignKey(o => o.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // إعداد علاقة العرض بالطلب (حذف العروض إذا تم حذف الطلب)
            modelBuilder.Entity<TaskOffer>()
                .HasOne(o => o.Task)
                .WithMany(t => t.Offers)
                .HasForeignKey(o => o.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. حقن التخصصات الأساسية
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "كهرباء", Description = "تمديدات وصيانة كهربائية عامة", IsActive = true },
                new Category { Id = 2, Name = "سباكة", Description = "تمديدات صحية وصيانة مواسير", IsActive = true },
                new Category { Id = 3, Name = "نجارة", Description = "أعمال خشبية وتصليح وتركيب أثاث", IsActive = true },
                new Category { Id = 4, Name = "دهان", Description = "طلاء جدران وأعمال ديكور", IsActive = true },
                new Category { Id = 5, Name = "تكييف وتبريد", Description = "صيانة وتنظيف وتركيب مكيفات", IsActive = true }
            );
        }
    }
}