using System.ComponentModel.DataAnnotations;

namespace Bravo.Models
{
    public class TaskMedia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TaskId { get; set; } = string.Empty;
        public ServiceTask? Task { get; set; }

        [Required, StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(50)]
        public string FileType { get; set; } = "image/jpeg";
    }
}