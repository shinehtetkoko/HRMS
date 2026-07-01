using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Services
{
    /// <summary>
    /// Provides services for managing and recording audit log entries within the system.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogService"/> class.
        /// </summary>
        /// <param name="context">The database context used for data operations.</param>
        public AuditLogService(AppDbContext context) => _context = context;

        /// <summary>
        /// Records a new action as an audit log entry in the database.
        /// </summary>
        /// <param name="performedById">The ID of the account that performed the action.</param>
        /// <param name="entity">The entity object associated with the action (e.g., User, LeaveRequest).</param>
        /// <param name="module">The name of the system module where the action occurred.</param>
        /// <param name="action">The type of action performed (e.g., Created, Updated, Deleted).</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task AddLogAsync(int performedById, object entity, string module, string action)
        {
            int? resolvedTargetUserId = entity switch
            {
                User user => user.User_Id,LeaveRequest request => request.User_Id, _ => null
            };
            var log = new AuditLog
            {
                Performed_Account_Id = performedById,
                Target_User_Id = resolvedTargetUserId,
                Module_Name = module,
                Action_Type = action,
                Created_At = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a list of audit logs filtered by role, day, and month.
        /// </summary>
        /// <param name="roleId">Optional: The role ID of the account that performed the actions.</param>
        /// <param name="day">Optional: The specific day of the month to filter logs by.</param>
        /// <param name="month">Optional: The specific month to filter logs by.</param>
        /// <returns>A task containing an enumerable collection of filtered audit log entries, ordered by creation date descending.</returns>
        public async Task<IEnumerable<AuditLog>> GetFilteredLogsAsync(int? roleId, int? day, int? month)
        {
            var query = _context.AuditLogs.Include(a => a.PerformedByAccount).ThenInclude(a => a.User).Include(a => a.PerformedByAccount).ThenInclude(a => a.Role) .Include(a => a.TargetUser).AsQueryable();

            if (roleId.HasValue)
            {
                query = query.Where(a => a.PerformedByAccount != null && a.PerformedByAccount.Role_Id == roleId.Value);
            }
            if (day.HasValue)
            {
                query = query.Where(a => a.Created_At.Day == day.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(a => a.Created_At.Month == month.Value);
            }
            return await query.OrderByDescending(a => a.Created_At).ToListAsync();
        }
    }
}