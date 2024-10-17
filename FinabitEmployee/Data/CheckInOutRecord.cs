using System.ComponentModel.DataAnnotations;

namespace FinabitEmployee.Data
{
    public class CheckInOutRecord
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } 
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
   

}
