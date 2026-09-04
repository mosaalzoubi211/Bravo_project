using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bravo.Models
{
    public class TaskOffer
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string TaskId { get; set; } = string.Empty;

        [ForeignKey(nameof(TaskId))]
        public ServiceTask? Task { get; set; }

        [Required]
        public string WorkerId { get; set; } = string.Empty;

        [ForeignKey(nameof(WorkerId))]
        public AppUser? Worker { get; set; }

        // السعر الذي يقترحه الحرفي لإنجاز المهمة
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProposedPrice { get; set; }

        // رسالة إقناع يكتبها الحرفي للعميل (مثلاً: أستطيع إنجازها اليوم بمواد أصلية)
        [StringLength(500)]
        public string? WorkerMessage { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // لمعرفة إذا كان العميل قد وافق على هذا العرض تحديداً
        public bool IsAccepted { get; set; } = false;
    }
}