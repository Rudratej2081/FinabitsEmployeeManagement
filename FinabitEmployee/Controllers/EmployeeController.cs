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
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext appDb)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = appDb;
        }
        //[HttpPut("update")]
        //[Authorize(Roles ="User")] 
        //public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeDTO model)
        //{

        //    var email = User.FindFirstValue(ClaimTypes.Email);
            
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null)
        //    {
        //        return NotFound("Employee not found.");
        //    }


        //    if (!string.IsNullOrEmpty(model.FirstName))
        //    {
        //        user.FirstName = model.FirstName;
        //    }
        //    if (!string.IsNullOrEmpty(model.Email))
        //    {
        //        user.Email = model.Email;
        //        user.UserName = model.Email;
        //    }
        //    if (!string.IsNullOrEmpty(model.LastName))
        //    {
        //        user.LastName = model.LastName;
        //    }

        //    if (!string.IsNullOrEmpty(model.Phone))
        //    {
        //        user.Phone = model.Phone;
        //    }
           

        //    var result = await _userManager.UpdateAsync(user);

        //    if (result.Succeeded)
        //    {
        //        return Ok(new { message = "Employee details updated successfully." });
        //    }

        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }

        //    return BadRequest(ModelState);
        //}
        [HttpPost("apply")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ApplyForLeave([FromBody] LeaveRequestDto leaveRequestDto)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;

            if (leaveRequestDto == null)
            {
                return BadRequest("Invalid leave request data.");
            }



            if (email == null)
            {
                return NotFound("User not found.");
            }


            var leaveRequest = new LeaveRequest
            {
                userId = userId,
                StartDate = leaveRequestDto.StartDate,
                EndDate = leaveRequestDto.EndDate,
                Reason = leaveRequestDto.Reason,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };


            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave request submitted successfully and is pending approval." });
        }

        [HttpPost("checkin")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CheckIn()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;


            if (userId == null || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid check-in details.");
            }

            var checkInRecord = new CheckInOutRecord
            {
               UserId=userId,
                CheckInTime = DateTime.UtcNow
            };

            await _context.checkincheckouts.AddAsync(checkInRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Checked in successfully." });
        }


        [HttpPost("checkout")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CheckOut()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;
            if (userId == null || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid check-out details.");
            }

            var record = await _context.checkincheckouts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CheckOutTime == null);

            if (record == null)
            {
                return NotFound("No active check-in record found.");
            }


            record.CheckOutTime = DateTime.UtcNow;
            _context.checkincheckouts.Update(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Checked out successfully." });
        }

        [HttpPut("UpdateDeatilsPhoto")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateEmployee([FromForm] UpdateEmployeeDTO model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("Employee not found.");
            }

            // Update user details
            if (!string.IsNullOrEmpty(model.FirstName)) user.FirstName = model.FirstName;
            if (!string.IsNullOrEmpty(model.LastName)) user.LastName = model.LastName;
            if (!string.IsNullOrEmpty(model.Email)) user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Phone)) user.Phone = model.Phone;

            // Handle profile picture if provided
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath);
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfilePicture.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicturePath = $"uploads/{uniqueFileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) return Ok(new { message = "Employee details and profile picture updated successfully." });

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }


        [Authorize(Roles = "User")]
        [HttpDelete("delete-profile-picture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            // Get the logged-in user's ID from claims
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized("User ID not found. Please log in again.");
            }

            // Fetch the user from the database
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the user has a profile picture to delete
            if (string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                return BadRequest("No profile picture to delete.");
            }

            // Construct the path to the existing profile picture
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath);

            // Attempt to delete the profile picture file
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath); // Delete the profile picture
                }
                catch (Exception ex)
                {
                    // Log the error or return an error response
                    return StatusCode(500, $"Error deleting profile picture: {ex.Message}");
                }
            }
            else
            {
                return NotFound("Profile picture file not found on the server.");
            }

            // Clear the profile picture path in the database
            user.ProfilePicturePath = null;
            var result = await _userManager.UpdateAsync(user);

            // Check if the user was updated successfully
            if (result.Succeeded)
            {
                return Ok(new { message = "Profile picture deleted successfully." });
            }

            // Return errors if the update failed
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }


    }
}
