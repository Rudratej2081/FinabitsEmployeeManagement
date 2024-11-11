//using FinabitEmployee.Data;
using FinabitEmployee.Data;
using FinabitEmployee.Migrations;
using FinabitEmployee.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace firstproj.Models.Entity
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<CheckInOutRecord> checkincheckouts { get; set; }
        public DbSet<DailyActivity> DailyActivities { get; set; }
        public DbSet<Attendance> AttendanceRecords { get; set; }
        public DbSet<Message> messages { get; set; }

        public DbSet<SalarySlip>salarySlips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<LeaveRequest>()
                .Property(lr => lr.Status)
                .HasConversion<int>();
           

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.UserId, a.Date }) 
                .IsUnique(); 

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.AttendanceRecords)
                .HasForeignKey(a => a.UserId);
        }
    }
}
