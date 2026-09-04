using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bravo.Models
{
    public class ServiceTask
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ClientId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClientId))] // تمت الإضافة هنا
        public AppUser? Client { get; set; }

        // يمكن أن يكون فارغاً إذا كان الطلب عاماً ولم يوافق عليه أحد بعد
        public string? WorkerId { get; set; }

        [ForeignKey(nameof(WorkerId))] // تمت الإضافة هنا
        public AppUser? Worker { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Location { get; set; } = string.Empty;

        // أضف هذا السطر قبل خاصية Status
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // دمج التاريخ والوقت لتجنب مشاكل المناطق الزمنية
        public DateTimeOffset ScheduledDate { get; set; }

        // تحديد دقة الأرقام العشرية لتجنب أخطاء SQL Server
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AgreedPrice { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // علاقة 1-إلى-متعدد لدعم رفع أكثر من صورة للمشكلة
        public ICollection<TaskMedia> MediaFiles { get; set; } = new List<TaskMedia>();
        // تقييم العميل للمهمة (من 1 إلى 5)
        public int? TaskRating { get; set; }

        // تعليق العميل (اختياري)
        [StringLength(500)]
        public string? ClientReview { get; set; }

        // نظام التقييم (من 1 إلى 5 نجوم)
        public int? ClientRating { get; set; }

        // تعليق العميل على الخدمة
        [MaxLength(500)]
        public ICollection<TaskOffer> Offers { get; set; } = new List<TaskOffer>();
      
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}