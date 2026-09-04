using Bravo.Models;
using Bravo.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Bravo.Data
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب ادخاله")]
        [Display(Name = "الاسم الكامل")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "الصورة الشخصية")]
        public string? ProfileImage { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // دور المستخدم (Client, Worker, Admin)
        public UserRole Role { get; set; }

        // خصائص إضافية للحرفيين
        public double Rating { get; set; } = 0.0;
        public bool IsAvailable { get; set; } = true;

        // العلاقات مع جداول المهام
        public ICollection<ServiceTask> ClientTasks { get; set; } = new List<ServiceTask>();
        public ICollection<ServiceTask> WorkerTasks { get; set; } = new List<ServiceTask>();
    }
}