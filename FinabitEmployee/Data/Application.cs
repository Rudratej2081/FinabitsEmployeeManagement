using Microsoft.AspNetCore.Identity;

public class Application : IdentityUser
{

    public IdentityRole userRole { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfJoining { get; set; }

    public string Phone { get; set; }
    public string? Designation { get; set; }
    public string Password { get; set; }
}

