using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bravo.Models
{
    public class AppUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        // نبذة تعريفية يكتبها الحرفي عن نفسه وعن خبراته الدقيقة
        [StringLength(500)]
        public string? Bio { get; set; }
        // تأكد من وجود هذه الخاصية تحديداً (الدور)
        public UserRole Role { get; set; }

        public double Rating { get; set; } = 0.0;

        // تأكد من وجود هذه الخاصية
        public bool IsAvailable { get; set; } = true;
        // ربط الحرفي بتخصص رئيسي (كهرباء، سباكة، إلخ)
        public int? CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }
        // تأكد من وجود هذه الخاصية
        public bool IsActive { get; set; } = true;

        public ICollection<ServiceTask> ClientTasks { get; set; } = new List<ServiceTask>();
        public ICollection<ServiceTask> WorkerTasks { get; set; } = new List<ServiceTask>();
    }
}