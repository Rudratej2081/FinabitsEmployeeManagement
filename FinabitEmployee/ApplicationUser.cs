using FinabitEmployee.Data;
using Microsoft.AspNetCore.Identity;

namespace FinabitEmployee.Models
{

    public class ApplicationUser : IdentityUser
    {
    
      
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfJoining { get; set; }

        public string Phone {  get; set; }
        public string? Designation {  get; set; }
        public string Password {  get; set; }

        public virtual ICollection<Attendance> AttendanceRecords { get; set; }
        public string? ProfilePicturePath { get; set; }
    }
    public class Application : ApplicationUser
    {

        public IdentityRole userRole { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfJoining { get; set; }

        public string Phone { get; set; }
        public string? Designation { get; set; }
        public string Password { get; set; }
    }

}
