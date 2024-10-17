namespace FinabitEmployee.Data
{
    public class UpdateEmployeeDTO
    {
        public string? FirstName { get; set; } 
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Designation { get; set; }

        public IFormFile? ProfilePicture { get; set; }
    }
}
