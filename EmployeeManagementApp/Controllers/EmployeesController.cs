using EmployeeManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagementApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeesController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Employees
        // We add a parameter 'searchString' that comes from the browser URL
        // GET: Employees
        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Include the "Department" table so we can show the Name later
            var employees = _context.Employees.Include(e => e.Department).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(s => s.FullName.Contains(searchString));
            }

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
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        // GET: Employees/Create
        [Authorize]
        public IActionResult Create()
        {
            // Fetch departments from DB. "Id" is the value saved, "Name" is the text shown.
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Check if a file was uploaded
                if (employee.ProfileImage != null)
                {
                    // 1. Create a folder path: wwwroot/images
                    string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                    // 2. Create a unique file name (so "john.jpg" doesn't overwrite another "john.jpg")
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + employee.ProfileImage.FileName;

                    // 3. Complete path
                    string filePath = Path.Combine(folder, uniqueFileName);

                    // 4. Copy the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await employee.ProfileImage.CopyToAsync(stream);
                    }

                    // 5. Save ONLY the file name to the database
                    employee.ProfilePicture = uniqueFileName;
                }

                _context.Add(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload Department list if error
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        // GET: Employees/Edit/5
        [Authorize]
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

            // CRITICAL FIX: We must load the dropdown from the Database!
            // The last parameter 'employee.DepartmentId' tells the dropdown: "Select this one by default"
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);

            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        // FIX: Bind "DepartmentId" instead of "Department"
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,DepartmentId,Salary,DateOfJoining,Email")] Employee employee)
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
                TempData["Success"] = "Employee Updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            // FIX: If validation fails, RELOAD the dropdown so it doesn't crash or go empty
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);

            return View(employee);
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin")] // <--- THE SECURITY GUARD
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
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
        [Authorize(Roles = "Admin")] // <--- DOUBLE LOCK
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
