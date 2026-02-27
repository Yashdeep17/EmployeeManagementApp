using EmployeeManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace EmployeeManagementApp.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender; // NEW: The Email Engine

        // Update constructor to accept IEmailSender
        public EmployeesController(AppDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _emailSender = emailSender; // Assign it
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
                // Check if we received cropped image data
                if (!string.IsNullOrEmpty(employee.CroppedImageData))
                {
                    // 1. The data looks like "data:image/png;base64,iVBORw0KGgo...", we only want the text AFTER the comma
                    var base64Data = Regex.Match(employee.CroppedImageData, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;

                    // 2. Convert text back to raw computer bytes
                    var bytes = Convert.FromBase64String(base64Data);

                    // 3. Create a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_profile.png";
                    string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string filePath = Path.Combine(folder, uniqueFileName);

                    // 4. Save the bytes as a real file on the server
                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                    // 5. Save the file name to the database
                    employee.ProfilePicture = uniqueFileName;

                }
                var user = new IdentityUser
                {
                    UserName = employee.Email,
                    Email = employee.Email,
                    EmailConfirmed = true // They haven't done the OTP yet!
                };

                // 2. Create the account with a standard default password
                // (We will force them to change this later)
                var result = await _userManager.CreateAsync(user, "Welcome@123!");

                if (result.Succeeded)
                {
                    // 1. Save the HR record to the database
                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    // ==========================================
                    // NEW: GENERATE OTP AND SEND EMAIL
                    // ==========================================

                    // 2. Generate a random 6-digit code
                    string otpCode = new Random().Next(100000, 999999).ToString();

                    // 3. Save the OTP securely in the Identity Tokens table
                    await _userManager.SetAuthenticationTokenAsync(user, "Default", "EmailOTP", otpCode);

                    // 4. Design the Welcome Email
                    string subject = "Welcome to HR Portal - Your Verification Code";
                    string htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; padding: 30px; text-align: center; background-color: #f8f9fa; border-radius: 10px;'>
                    <h2 style='color: #212529;'>Welcome to the Team!</h2>
                    <p style='color: #6c757d; font-size: 16px;'>Your corporate profile has been successfully provisioned.</p>
                    <p style='font-size: 16px;'>Your temporary login password is: <b style='color: #0d6efd;'>Welcome@123!</b></p>
                    <div style='margin: 30px 0;'>
                        <p style='color: #6c757d; margin-bottom: 5px;'>Your 6-digit verification code is:</p>
                        <h1 style='color: #8bc34a; letter-spacing: 8px; font-size: 40px; margin: 0;'>{otpCode}</h1>
                    </div>
                    <p style='color: #6c757d; font-size: 14px;'>Please log in to the portal and enter this code to verify your account and change your password.</p>
                </div>";

                    // 5. Fire off the email!
                    // 5. Fire off the email safely and catch any errors!
                    try
                    {
                        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
                        TempData["Success"] = "Employee created and OTP Email sent successfully!";
                    }
                    catch (Exception ex)
                    {
                        // If Google blocks the email, it will save the exact error reason here
                        TempData["Error"] = "Email failed to send: " + ex.Message;
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // If the password was too weak or email already exists, show the error
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(employee);
                }
            }
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
            TempData["Success"] = "Employee deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
