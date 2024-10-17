﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using FinabitEmployee.Models; 
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using FinabitEmployee.Data;
using firstproj.Models.Entity;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext appDb)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = appDb;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("add")]
    public async Task<IActionResult> AddEmployee([FromForm] AddEmployeeDto model)
    {
        if (model == null)
        {
            return BadRequest("Invalid employee data.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

       
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        string profilePictureFilePath = null;

        
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            // Generate a unique file name to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfilePicture.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

            // Ensure the "uploads" folder exists
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"));
            }

            // Save the profile picture to the file system
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfilePicture.CopyToAsync(stream);
            }

            // Assign file path to profile picture
            profilePictureFilePath = $"uploads/{uniqueFileName}";
        }

        // Create the new employee (ApplicationUser)
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            Password = model.Password, 
            Designation = model.Designation,
            DateOfJoining = model.DateOfJoining,
            ProfilePicturePath = profilePictureFilePath 
        };

        // Create the user and assign the role
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { message = "Employee added successfully." });
        }

        // Handle errors
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }


    [HttpGet("list")]
    [Authorize(Roles ="Admin")] 
    public async Task<IActionResult> GetEmployeeList(int pageNumber = 1, int pageSize = 10)
    {
    
        var totalEmployees = await _userManager.Users.CountAsync();

        
        var employees = await _userManager.Users
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize) 
            .Take(pageSize) 
            .Select(user => new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Phone,
                user.DateOfJoining,
                user.Designation
            })
            .ToListAsync();

        // Create a response object
        var response = new
        {
            TotalCount = totalEmployees,
            PageSize = pageSize,
            CurrentPage = pageNumber,
            Employees = employees
        };

        return Ok(response);
    }


    [HttpDelete("delete/{username}")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> DeleteEmployee(string username)
    {
 
        var user = await _userManager.FindByEmailAsync(username);

        if (user == null)
        {
            return NotFound(new { message = "Employee not found." });
        }

       
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            
            return BadRequest(new { message = "Failed to delete the employee.", errors = result.Errors });
        }

       
        return Ok(new { message = "Employee deleted successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
        {
            return BadRequest("Invalid login request.");
        }


        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        
        var passwordCheck = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordCheck)
        {
            return Unauthorized("Invalid email or password.");
        }

  
        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any())
        {
            return Unauthorized("No roles assigned to the user.");
        }

        
        var token = GenerateJwtToken(user, roles);

        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
       // new Claim(ClaimTypes.Role, user.userRole.Name)


    };

        // Add user roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPut("employee-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeDTO model)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound("Employee not found.");
        }


        if (!string.IsNullOrEmpty(model.FirstName))
        {
            user.FirstName = model.FirstName;
        }

        if (!string.IsNullOrEmpty(model.LastName))
        {
            user.LastName = model.LastName;
        }
        if (!string.IsNullOrEmpty(model.Email))
        {
            user.Email = model.Email;
        }

        if (!string.IsNullOrEmpty(model.Phone))
        {
            user.Phone = model.Phone;
        }
        if (!string.IsNullOrEmpty(model.Designation))
        {
            user.Designation = model.Designation;
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Ok(new { message = "Employee details updated successfully." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpGet("leaves")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllLeaveRequests(string employeeName = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Start with the queryable data from the LeaveRequests table and join with the AspNetUsers table
        var query = _context.LeaveRequests
            .AsNoTracking()
            .Join(
                _context.Users,            // Join with the AspNetUsers table
                leave => leave.userId,      // Link LeaveRequests.UserId with AspNetUsers.Id
                user => user.Id,
                (leave, user) => new        // Project the result into an anonymous object
                {
                    leave.Id,
                    leave.StartDate,
                    leave.EndDate,
                    leave.Status,
                    leave.Reason,
                    EmployeeName = user.FirstName + " " + user.LastName // Combine first and last names
                })
            .AsQueryable();

        // Filter by employee name if provided
        if (!string.IsNullOrEmpty(employeeName))
        {
            // Convert both employeeName and EmployeeName to lowercase for case-insensitive comparison
            employeeName = employeeName.ToLower();
            query = query.Where(leave => (leave.EmployeeName.ToLower()).Contains(employeeName));
        }

        // Filter by start date if provided
        if (startDate.HasValue)
        {
            query = query.Where(leave => leave.StartDate >= startDate.Value);
        }

        // Filter by end date if provided
        if (endDate.HasValue)
        {
            query = query.Where(leave => leave.EndDate <= endDate.Value);
        }

        // Execute the query and get the list of leave requests
        var leaveRequests = await query.ToListAsync();

        return Ok(leaveRequests);
    }





    [HttpPost("leave-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLeaveStatus([FromBody] UpdateLeaveStatusDto model)
    {
        
        if (model == null || model.Id <= 0)
        {
            return BadRequest("Invalid leave request details.");
        }

       
        var leaveRequest = await _context.LeaveRequests.FindAsync(model.Id);
        if (leaveRequest == null)
        {
            return NotFound("Leave request not found.");
        }

    
        if (model.Status != LeaveStatus.Approved && model.Status != LeaveStatus.Rejected)
        {
            return BadRequest("Invalid status provided. Only 'Approved' or 'Rejected' are allowed.");
        }


        leaveRequest.Status = model.Status;
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Leave request status updated successfully." });
    }

}







