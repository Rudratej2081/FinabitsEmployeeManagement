public class AddEmployeeDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Designation { get; set; }
    public decimal CTC {  get; set; }
    public string Phone {  get; set; }
    public DateTime DateOfJoining { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}
