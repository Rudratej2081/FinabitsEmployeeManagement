using FinabitEmployee.Data;
using FinabitEmployee.Models;
using firstproj.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinabitEmployee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChattingController : ControllerBase
    {
       
            private readonly AppDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly IConfiguration _configuration;
            private readonly FaceRecognitionService _faceRecognitionService;
            private readonly IHubContext<ChatHub> _hubContext;

            public ChattingController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext appDb, IHubContext<ChatHub> hubContext)
            {
                _hubContext = hubContext;
                _userManager = userManager;
                _roleManager = roleManager;
                _configuration = configuration;
                _context = appDb;
                _faceRecognitionService = new FaceRecognitionService();
            }
            [HttpGet("GetChatUsers")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetChatUsers()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var currentUserId = user.Id;

            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new ApplicationUser // Assuming ApplicationDto has properties for Id and Email
                {
                    Id = u.Id,
                    Email = u.Email
                    // Add any other properties from ApplicationDto you need
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpPost("send")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var senderId = user.Id;

            if (user == null)
                return NotFound("User not found.");

            var message = new Message
            {
                Content = messageDto.Content,
                SentAt = DateTime.UtcNow,
                SenderId = senderId,  // Ensure you're using the authenticated user ID
                ReceiverId = messageDto.ReceiverId,
            };

            _context.messages.Add(message);
            await _context.SaveChangesAsync();

            // Notify the receiver about the new message
            await _hubContext.Clients.User(messageDto.ReceiverId).SendAsync("ReceiveMessage", user.UserName, messageDto.Content);

            // Optionally return the message
            return Ok(message);
        }
        [HttpGet("messages/{receiverId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMessages(string receiverId)
        {
            // Retrieve the currently authenticated user's email
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Find the user by their email
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user?.Id;

            // Check if userId and receiverId are valid
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(receiverId))
            {
                return BadRequest("Invalid user ID or receiver ID.");
            }

            // Fetch messages exchanged between the two users
            var messages = await _context.messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == receiverId) ||
                             (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Check if messages were found
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found between the specified users.");
            }

            // Categorize messages into sent and received
            var response = new
            {
                SentMessages = messages
                    .Where(m => m.SenderId == userId)
                    .Select(m => new { m.Content, m.SentAt }) // Include necessary properties
                    .ToList(),
                ReceivedMessages = messages
                    .Where(m => m.SenderId == receiverId)
                    .Select(m => new { m.Content, m.SentAt }) // Include necessary properties
                    .ToList()
            };

            return Ok(response);
        }



        [HttpGet("messages")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetSentAndReceivedMessages()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);


            // Find the logged-in user by email

            if (user == null)
            {
                return NotFound("Logged-in user not found.");
            }

            var userId = user?.Id;

            // Retrieve messages where the user is either the sender or the receiver
            var messages = await _context.messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Check if messages were found
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found for the logged-in user.");
            }

            return Ok(messages);
        }
        [HttpGet("currentUser")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get the user ID from the token
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByEmailAsync(email);
            var userId = user?.Id;

            if (userId == null)
            {
                return Unauthorized("User not found.");
            }

            // Find the user in the database


            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Return the user details (customize the properties as needed)
            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName
            });
        }

    }
}
