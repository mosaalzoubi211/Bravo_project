using System.ComponentModel.DataAnnotations;

namespace Bravo.ViewModels
{
    public class WorkerProfileViewModel
    {
        [Display(Name = "التخصص الرئيسي")]
        [Required(ErrorMessage = "يرجى اختيار التخصص الذي تعمل به")]
        public int CategoryId { get; set; }

        [Display(Name = "نبذة تعريفية (Bio)")]
        [Required(ErrorMessage = "يرجى كتابة نبذة عن خبراتك وخدماتك")]
        [StringLength(500, ErrorMessage = "النبذة يجب ألا تتجاوز 500 حرف")]
        public string Bio { get; set; } = string.Empty;
    }
}