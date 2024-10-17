using FinabitEmployee.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LeaveRequest
{
    [Key]
    public int Id { get; set; }

    public string userId {  get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; }

   
    public LeaveStatus Status { get; set; } 

    public DateTime CreatedAt { get; set; }

    
}
public enum LeaveStatus
{
    Pending = 0, 
    Approved = 1,
    Rejected = 2
}
public class UpdateLeaveStatusDto
{
    public int Id { get; set; }            
    public LeaveStatus Status { get; set; }  
}
