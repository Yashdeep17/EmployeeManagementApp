using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Need this for [NotMapped]
using Microsoft.AspNetCore.Http;

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
        // Foreign Key (This stores the ID, e.g., 1, 2, 5)
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        // Navigation Property (This lets us access the related Department object like 'emp.Department.Name')
        public Department? Department { get; set; }

        [Required]
        [EmailAddress] // Checks for @ symbol and domain
        [Display(Name = "Office Email")]
        public string Email { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Salary { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoining { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; } // Stores "john.jpg"

        [NotMapped] // This tells EF Core: "Do not create a column for this!"
        [Display(Name = "Upload Image")]
        public IFormFile? ProfileImage { get; set; } // Handles the actual file upload
    }
}