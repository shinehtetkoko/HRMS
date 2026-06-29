using HRMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRMS.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Retrieves the target user ID based on the entity type.
        /// </summary>
        /// <param name="entity">The entity being processed.</param>
        /// <returns>The associated user ID if found; otherwise, null.</returns>
        /// <summary>
        /// Returns the target user id for audit log.
        /// </summary>
        private int? GetTargetUserId(object entity)
        {
            return entity switch
            {
                User user => user.User_Id,
                LeaveRequest request => request.User_Id,
                _ => null
            };
        }

        /// <summary>
        /// Saves all pending changes and automatically creates audit logs.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //----------------------------------------------------
            // Logged-in Account_Id
            //----------------------------------------------------
            int performedAccountId = 1;
            var accountIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(accountIdClaim))
            {
                int.TryParse(accountIdClaim, out performedAccountId);
                if (performedAccountId <= 0) performedAccountId = 1;
            }
            //----------------------------------------------------
            // Get Changed Entries
            //----------------------------------------------------
            var entries = ChangeTracker.Entries().Where(entry =>
            {
                // Skip AuditLog
                if (entry.Entity is AuditLog)
                {
                    return false;
                }
                // Only Added / Modified / Deleted
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified && entry.State != EntityState.Deleted)
                {
                    return false;
                }
                //------------------------------------------------
                // Leave Request
                //------------------------------------------------
                if (entry.Entity is LeaveRequest)
                {
                    // Log only Approved / Rejected
                    if (entry.State != EntityState.Modified)
                    {
                        return false;
                    }

                    var statusProperty = entry.Property(nameof(LeaveRequest.status));
                    if (!statusProperty.IsModified)
                    {
                        return false;
                    }

                    string? status = statusProperty.CurrentValue?.ToString();

                    return status == "Approved" || status == "Rejected";
                }
                //------------------------------------------------
                // Other Modules
                //------------------------------------------------
                return entry.Entity is LeavePolicy || entry.Entity is LeaveType || entry.Entity is Company || entry.Entity is Department || entry.Entity is User;
            })
                .ToList();
            //----------------------------------------------------
            // Create Audit Logs
            //----------------------------------------------------
            foreach (var entry in entries)
            {
                string actionType = entry.State.ToString();
                if (entry.Entity is LeaveRequest request)
                {
                    actionType = request.status;
                }
                AuditLogs.Add(new AuditLog
                {
                    Performed_Account_Id = performedAccountId,
                    Target_User_Id = GetTargetUserId(entry.Entity),
                    Module_Name = entry.Entity.GetType().Name,
                    Action_Type = actionType,
                    Created_At = DateTime.UtcNow
                });
            }
            //----------------------------------------------------
            // Save
            //----------------------------------------------------
            return await base.SaveChangesAsync(cancellationToken);
        }
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

    }
}