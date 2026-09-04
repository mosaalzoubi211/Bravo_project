using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bravo.Models
{
    public class ChatMessage
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

   
        [Required]
        public string TaskId { get; set; } = string.Empty;

        [ForeignKey(nameof(TaskId))]
        public ServiceTask? Task { get; set; }

        // المرسل (قد يكون العميل أو الحرفي)
        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public AppUser? Sender { get; set; }

        // المستقبل (الطرف الآخر)
        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [ForeignKey(nameof(ReceiverId))]
        public AppUser? Receiver { get; set; }

        // محتوى الرسالة
        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

        // لمعرفة هل تمت قراءة الرسالة أم لا
        public bool IsRead { get; set; } = false;
    }
}