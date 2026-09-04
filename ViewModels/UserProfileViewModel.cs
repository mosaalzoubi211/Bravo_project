using System.ComponentModel.DataAnnotations;

namespace Bravo.ViewModels
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "العنوان (المدينة / الحي)")]
        public string? Address { get; set; }

        // خاصية مخفية لمعرفة هل المستخدم حرفي أم عميل لكي نعرض الحقول المناسبة
        public bool IsWorker { get; set; }

        [Display(Name = "التخصص")]
        public int? CategoryId { get; set; }

        [Display(Name = "نبذة عنك (Bio)")]
        public string? Bio { get; set; }
    }
}