using System.ComponentModel.DataAnnotations;

namespace FinabitEmployee.Data
{
    public class DailyActivity
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }  
        public DateTime Date { get; set; }  
        public string Description { get; set; }  
        public string HoursWorked { get; set; }
    }
    public class DailyActivityDto
    {
        public string Description { get; set; }
        public string HoursWorked { get; set; }
        
    }
}
