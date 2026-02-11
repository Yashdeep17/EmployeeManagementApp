using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        // Initialize with empty string to satisfy the compiler
        public string FullName { get; set; } = string.Empty;

        [Required]
        // Initialize with empty string
        public string Department { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Salary { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoining { get; set; }
    }
}