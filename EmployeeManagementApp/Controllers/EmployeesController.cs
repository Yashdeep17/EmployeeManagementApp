using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementApp.Models;

namespace EmployeeManagementApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        // We add a parameter 'searchString' that comes from the browser URL
        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Start with ALL employees (but don't fetch them yet)
            var employees = from e in _context.Employees
                            select e;

            // 2. If the user typed something, filter the list
            if (!String.IsNullOrEmpty(searchString))
            {
                // This generates SQL: WHERE FullName LIKE '%searchString%'
                employees = employees.Where(s => s.FullName.Contains(searchString));
            }

            // 3. Execute the query and send the list to the View
            return View(await employees.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            // 1. Create a list of Departments (In the real world, this comes from a database)
            List<string> departments = new List<string> { "IT", "HR", "Finance", "Marketing", "Sales", "Development" };

            // 2. Put it in the "ViewBag" (a backpack to carry data to the View)
            // We use "SelectList" because the Dropdown needs a specific format
            ViewBag.DeptList = new SelectList(departments);
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CHANGE 1: Add "Email" to this list!
        public async Task<IActionResult> Create([Bind("Id,FullName,Department,Salary,DateOfJoining,Email")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // CHANGE 2: If validation fails (e.g. error), we MUST reload the dropdown list
            // Otherwise the dropdown comes back empty!
            List<string> departments = new List<string> { "IT", "HR", "Finance", "Marketing", "Sales" };
            ViewBag.DeptList = new SelectList(departments, employee.Department);

            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // 1. Create a list of Departments (In the real world, this comes from a database)
            List<string> departments = new List<string> { "IT", "HR", "Finance", "Marketing", "Sales", "Development" };

            // 2. Put it in the "ViewBag" (a backpack to carry data to the View)
            // We use "SelectList" because the Dropdown needs a specific format
            // The second argument 'employee.Department' tells the list which item to select by default
            ViewBag.DeptList = new SelectList(departments, employee.Department);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CHANGE 1: Add "Email" here too
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Department,Salary,DateOfJoining,Email")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // CHANGE 2: Reload the dropdown list here too!
            List<string> departments = new List<string> { "IT", "HR", "Finance", "Marketing", "Sales" };
            ViewBag.DeptList = new SelectList(departments, employee.Department);

            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
