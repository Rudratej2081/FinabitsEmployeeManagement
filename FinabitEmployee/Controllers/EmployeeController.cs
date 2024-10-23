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
        private readonly FaceRecognitionService _faceRecognitionService;

        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext appDb)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = appDb;
            _faceRecognitionService = new FaceRecognitionService();
        }
        
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
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "C:\\FinabitsEmployee", user.ProfilePicturePath);
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfilePicture.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "C:\\FinabitsEmployee", uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicturePath = $"{uniqueFileName}";
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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "C:\\FinabitsEmployee", user.ProfilePicturePath);

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
       
        [Authorize(Roles = "User")]
        [HttpPost("daily-activity")]
        public async Task<IActionResult> LogDailyActivity([FromBody] DailyActivityDto activityDto)
        {
       
            // Get the logged-in user's ID from the claims
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;

            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            // Create a new activity log entry
            var activityLog = new DailyActivity
            {
                UserId = userId,
                Description = activityDto.Description,
                HoursWorked=activityDto.HoursWorked,
                Date = DateTime.UtcNow
            };

            // Save the activity to the database
            _context.DailyActivities.Add(activityLog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Daily activity logged successfully." });
        }


        [Authorize(Roles ="User")]       
        [HttpPut("updateactivity{id}")]
        public async Task<IActionResult> UpdateActivity(int id,[FromBody] DailyActivityDto updatedActivity)
        {
            // Get the logged-in user's ID
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = new DailyActivity
            {
                Id= id,
                UserId = user.Id,
                
            };
            

            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            // Find the activity by ID
            var activity = await _context.DailyActivities.FindAsync(id);


            if (activity == null)
            {
                return NotFound("Activity not found or not owned by the user.");
            }

            // Update activity fields
            activity.Description = updatedActivity.Description;
            activity.HoursWorked = updatedActivity.HoursWorked;
            activity.Date = DateTime.Now;

            _context.DailyActivities.Update(activity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Activity updated successfully." });
        }

        [Authorize(Roles = "User")]
        [HttpDelete("deleteactivity{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {

            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user.Id;

            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

         
            var activity = await _context.DailyActivities.FindAsync(id);

            if (activity == null || activity.UserId != userId)
            {
                return NotFound("Activity not found or not owned by the user.");
            }

            _context.DailyActivities.Remove(activity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Activity deleted successfully." });
        }
        //[HttpPost("compare")]
        //[Authorize(Roles = "User")]
        //public async Task<IActionResult> CompareFaces([FromBody] CompareRequest request)
        //{
        //    // Get the user's email from claims
        //    var email = User.FindFirstValue(ClaimTypes.Email);

        //    // Find the user by email
        //    var user = await _userManager.FindByEmailAsync(email);

        //    if (user == null)
        //    {
        //        return Unauthorized("Unable to find the user.");
        //    }

        //    // Define the folder where profile pictures are saved
        //    var profileImagePath = Path.Combine(@"C:\FinabitsEmployee", user.ProfilePicturePath);

        //    // Check if the profile image exists
        //    if (!System.IO.File.Exists(profileImagePath))
        //    {
        //        return NotFound("Profile image not found.");
        //    }

        //    // Decode the base64 image
        //    var base64Image = request.Image.Replace("data:image/jpeg;base64,", ""); // Adjust this if your image type is different
        //    var webcamImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");

        //    // Save the decoded image to a file
        //    System.IO.File.WriteAllBytes(webcamImagePath, Convert.FromBase64String(base64Image));

        //    // Compare the two images: profile image and webcam image
        //    var result = _faceRecognitionService.CompareFaces(profileImagePath, webcamImagePath);

        //    // Clean up the temporary file after comparison
        //    System.IO.File.Delete(webcamImagePath);

        //    if (result)
        //    {
        //        return Ok(new { message = "Faces match." });
        //    }

        //    return BadRequest(new { message = "Faces does not match." });
        //}

        // Define a class to receive the compare request


        [HttpPost("attendance")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateAttendance([FromBody] CompareRequest request)
        {
            // Get the user's email from claims
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            // Check if the user exists
            if (user == null)
            {
                return Unauthorized("Unable to find the user.");
            }

            var today = DateTime.UtcNow.Date; // Get today's date
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(ar => ar.UserId == user.Id && ar.Date == today);

            // Call the CompareFaces method to check face match
            var compareResponse = await CompareFaces(request);

            // Determine if the face match was successful
            bool isFaceMatched = compareResponse is OkObjectResult; // Check if the response was OK
           
            if (existingRecord != null)
            {
                
                // Update existing record
                if (isFaceMatched)
                {
                    existingRecord.IsPresent = true;
                    existingRecord.IsAbsent = false;
                    existingRecord.ConsecutiveMismatches = 0; // Reset on match
                }
                else
                {
                    existingRecord.ConsecutiveMismatches++;

                    // Check if consecutive mismatches reached 2
                    if (existingRecord.ConsecutiveMismatches >= 3)
                    {
                        existingRecord.IsPresent = false;
                        existingRecord.IsAbsent = true;

                        _context.AttendanceRecords.Update(existingRecord);
                        await _context.SaveChangesAsync();
                        return Ok(new { message = "Absentee Marked" });
                    }
                }

                // Update the existing record
                _context.AttendanceRecords.Update(existingRecord);
            }
            else
            {
                // Create a new record
                var newRecord = new Attendance
                {
                    UserId = user.Id,
                    Date = today,
                    IsPresent = isFaceMatched,
                    IsAbsent = !isFaceMatched,
                    ConsecutiveMismatches = isFaceMatched ? 0 : 1 // Set to 1 if face does not match
                };

                // Add the new record
                await _context.AttendanceRecords.AddAsync(newRecord);
            }

           
                await _context.SaveChangesAsync();
            
           

            return Ok(new { message = "Attendance updated successfully." });
        }


        // Compare Faces API
        [HttpPost("compare")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CompareFaces([FromBody] CompareRequest request)
        {
            // Get the user's email from claims
            var email = User.FindFirstValue(ClaimTypes.Email);

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized("Unable to find the user.");
            }

            // Define the folder where profile pictures are saved
            var profileImagePath = Path.Combine(@"C:\FinabitsEmployee", user.ProfilePicturePath);

            // Check if the profile image exists
            if (!System.IO.File.Exists(profileImagePath))
            {
                return NotFound("Profile image not found.");
            }

            // Decode the base64 image
            var base64Image = request.Image.Replace("data:image/jpeg;base64,", ""); // Adjust this if your image type is different
            var webcamImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");

            // Save the decoded image to a file
            System.IO.File.WriteAllBytes(webcamImagePath, Convert.FromBase64String(base64Image));

            // Compare the two images: profile image and webcam image
            var result = _faceRecognitionService.CompareFaces(profileImagePath, webcamImagePath);

            // Clean up the temporary file after comparison
            System.IO.File.Delete(webcamImagePath);

            if (result)
            {
                return Ok(new { message = "Faces match." });
            }

            return BadRequest(new { message = "Faces do not match." });
        }

        // Define a class to receive the compare request
        public class CompareRequest
        {
            public string Image { get; set; }
        }

    }

}

