using System.ComponentModel.DataAnnotations;

namespace Bravo.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
    }
}