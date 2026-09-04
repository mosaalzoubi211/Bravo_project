using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // ضروري للتعامل مع رفع الصور

namespace Bravo.ViewModels
{
    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "يرجى اختيار التخصص")]
        [Display(Name = "نوع الخدمة المطلوبة")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "يرجى كتابة وصف المشكلة بوضوح")]
        [StringLength(500)]
        [Display(Name = "وصف المشكلة")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى إدخال العنوان التفصيلي")]
        [StringLength(250)]
        [Display(Name = "العنوان (المدينة، الحي، الشارع)")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى تحديد موعد مبدئي")]
        [Display(Name = "الموعد المقترح للزيارة")]
        public DateTimeOffset ScheduledDate { get; set; } = DateTimeOffset.Now.AddDays(1);

        // خاصية رفع صورة للمشكلة (جعلناها اختيارية)
        [Display(Name = "إرفاق صورة للمشكلة (اختياري)")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "يرجى تحديد طريقة الدفع")]
        [Display(Name = "طريقة الدفع")]
        public Bravo.Models.PaymentMethod PaymentMethod { get; set; }
    }
}