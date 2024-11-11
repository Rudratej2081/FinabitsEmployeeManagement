using FinabitEmployee.Models;
using firstproj.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinabitEmployee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetManagement : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly FaceRecognitionService _faceRecognitionService;


        public AssetManagement(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext appDb)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = appDb;
            _faceRecognitionService = new FaceRecognitionService();
        }
    }
}
