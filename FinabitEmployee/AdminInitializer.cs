using FinabitEmployee.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class AdminInitializer
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAdminAsync(string adminEmail, string adminPassword)
    {
        
        var roleExists = await _roleManager.RoleExistsAsync("Admin");
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

       
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {

            adminUser = new ApplicationUser
            {
                FirstName = adminEmail,
                LastName = "FinabitsAdmin",
                UserName = adminEmail,
                Email = adminEmail,
                DateOfJoining = DateTime.Now,
                Password=adminPassword,
                Phone = "234567111",
                Designation ="Manager",
                ProfilePicturePath="null",
                EmailConfirmed = true

            };


            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
             
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
             
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }
    }
}
