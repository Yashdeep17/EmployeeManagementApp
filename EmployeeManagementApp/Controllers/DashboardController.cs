using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementApp.Models;

namespace EmployeeManagementApp.Controllers
{
    // We only want Admins looking at company payroll data!
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // 1. Calculate basic totals
            viewModel.TotalEmployees = await _context.Employees.CountAsync();
            viewModel.TotalDepartments = await _context.Departments.CountAsync();

            // 2. Calculate the total salary burn rate
            viewModel.TotalSalaryExpense = await _context.Employees.SumAsync(e => e.Salary);

            // 3. Group Employees by Department for our Pie Chart
            // This generates SQL similar to: SELECT Department, COUNT(*) GROUP BY Department
            var headcounts = await _context.Employees
                .Include(e => e.Department)
                .GroupBy(e => e.Department.Name)
                .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                .ToListAsync();

            // Load the grouped data into our dictionary
            foreach (var item in headcounts)
            {
                viewModel.DepartmentHeadcounts.Add(item.DepartmentName, item.Count);
            }

            return View(viewModel);
        }
    }
}