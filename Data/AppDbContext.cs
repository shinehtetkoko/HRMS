using HRMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Resignation> Resignations { get; set; }
        public DbSet<ProfileUpdateRequest> ProfileUpdateRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Department>().ToTable("tbl_department");
            modelBuilder.Entity<User>().ToTable("tbl_user");
            modelBuilder.Entity<User>()
             .HasOne(u => u.Department)
             .WithMany(d => d.Users)
             .HasForeignKey(u => u.Dept_Id)
             .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Department>()
            .HasOne(d => d.DeptHeadUser)
            .WithMany()
            .HasForeignKey(d => d.DeptHeadUserId)
            .OnDelete(DeleteBehavior.SetNull);
        }

    }
}