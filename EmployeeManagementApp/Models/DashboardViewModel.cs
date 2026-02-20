namespace EmployeeManagementApp.Models
{
    public class DashboardViewModel
    {
        // Top-level stats
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public decimal TotalSalaryExpense { get; set; }

        // Data for our Charts (e.g., "IT": 5, "HR": 2)
        // We use a Dictionary to hold the Department Name (string) and the Count (int)
        public Dictionary<string, int> DepartmentHeadcounts { get; set; } = new Dictionary<string, int>();
    }
}