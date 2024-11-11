using FinabitEmployee.Data;
using FinabitEmployee.Models;
using firstproj.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinabitEmployee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalarySlipManagement : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly FaceRecognitionService _faceRecognitionService;


        public SalarySlipManagement(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext appDb)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = appDb;
            _faceRecognitionService = new FaceRecognitionService();
        }
        [Authorize(Roles = "User, Admin")]
        [HttpGet("get-salary-slips")]
        public async Task<IActionResult> GetSalarySlips(
    [FromQuery] string email = null,
    [FromQuery] int? month = null,
    [FromQuery] int? year = null,
    [FromQuery] string employeeName = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var loggedInUserEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user?.Id;

            
            var salarySlipsQuery = _context.salarySlips.AsQueryable();

            
            if (!string.IsNullOrEmpty(email))
            {
                salarySlipsQuery = salarySlipsQuery.Where(s => s.Email == email);
            }
            else
            {
                salarySlipsQuery = salarySlipsQuery.Where(s => s.Email == loggedInUserEmail);
            }

            // Filter by employee name, available only for Admin (if employeeName is provided)
            if (!string.IsNullOrEmpty(employeeName))
            {
                salarySlipsQuery = salarySlipsQuery.Where(s => (s.FirstName + " " + s.LastName).Contains(employeeName));
            }

            // Filter by month if provided
            if (month.HasValue)
            {
                salarySlipsQuery = salarySlipsQuery.Where(s => s.Month == month.Value);
            }

            // Filter by year if provided
            if (year.HasValue)
            {
                salarySlipsQuery = salarySlipsQuery.Where(s => s.Year == year.Value);
            }

            // Implement pagination
            var salarySlipsPaged = await salarySlipsQuery
                .Skip((pageNumber - 1) * pageSize)  // Skip records for previous pages
                .Take(pageSize)                     // Take the specified page size
                .ToListAsync();

            // Get the total count for pagination information
            var totalCount = await salarySlipsQuery.CountAsync();

            if (salarySlipsPaged.Count == 0)
            {
                return NotFound("No salary slips found.");
            }

            // Return paginated response
            var result = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                SalarySlips = salarySlipsPaged
            };

            return Ok(result);
        }



        [Authorize(Roles = "User")]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSalarySlip([FromBody] SalarySlipRequest request)
        {
            // Retrieve the logged-in user's Employee ID
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var employeeId = user.Id;

            if (employeeId == null) return Unauthorized("User not authorized.");
            var existingSalarySlip = await _context.salarySlips
           .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Month == request.Month && s.Year == request.Year);

            if (existingSalarySlip != null)
            {
                return BadRequest("Salary slip already generated for this month and year.");
            }

            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null) return NotFound("Employee not found.");

            // Salary Calculation Logic
            decimal basicPay = employee.CTC * 0.4m / 12;
            decimal hra = basicPay * 0.25m;
            decimal deductions = basicPay * 0.1m;
            decimal netPay = basicPay + hra - deductions;

            var salarySlip = new SalarySlip
            {
                EmployeeId = employeeId,
                Email = employee.Email,
                Phonenumber = employee.PhoneNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Month = request.Month,
                Year = request.Year,
                BasicPay = basicPay,
                HRA = hra,
                Deductions = deductions,
                NetPay = netPay,
                GeneratedDate = DateTime.Now
            };

            await _context.salarySlips.AddAsync(salarySlip);
            await _context.SaveChangesAsync();

            return Ok("Salary slip generated successfully.");
        }
        [HttpGet("download")]
        public async Task<IActionResult> DownloadSalarySlip(int month, int year)
        {
            // Validate month and year
            if (month < 1 || month > 12) return BadRequest("Invalid month.");
            if (year < 1900 || year > DateTime.UtcNow.Year) return BadRequest("Invalid year.");

            // Get the logged-in user's email
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (email == null) return Unauthorized("User not authorized.");

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized("User not found.");

            // Get employee ID
            var employeeId = user.Id;

            // Find the salary slip
            var slip = await _context.salarySlips
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Month == month && s.Year == year);

            if (slip == null) return NotFound("Salary slip not found.");

            try
            {
                // Generate the PDF
                var pdfGenerator = new SalarySlipGenerator();
                byte[] pdf = await pdfGenerator.GeneratePdfAsync(slip); // Updated to use async if applicable

                // Return the PDF file
                return File(pdf, "application/pdf", $"SalarySlip_{month}_{year}.pdf");
            }
            catch (Exception ex)
            {
                // Log the exception (if logging is enabled) or return a meaningful error
                return StatusCode(500, "An error occurred while generating the salary slip.");
            }
        }
    }
}
