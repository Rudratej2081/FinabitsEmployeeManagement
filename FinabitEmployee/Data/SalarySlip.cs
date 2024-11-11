using FinabitEmployee.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;

namespace FinabitEmployee.Data
{
    public class SalarySlip
    {
        [Key]
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Phonenumber {  get; set; }
        public string Email { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicPay { get; set; }
        public decimal HRA { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public DateTime GeneratedDate { get; set; }


    }
    public class SalarySlipRequest
    {
       
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
