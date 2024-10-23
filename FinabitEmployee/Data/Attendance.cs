using FinabitEmployee.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinabitEmployee.Data
{
    public class Attendance
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } 

        public DateTime Date { get; set; } 

        public bool IsPresent { get; set; } 

        public bool IsAbsent { get; set; }  
        public int ConsecutiveMismatches { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
