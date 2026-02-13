using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty; // e.g., "HR", "IT"

        [Display(Name = "Department Code")]
        public string Code { get; set; } = string.Empty; // e.g., "HR-01"
    }
}