using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using HRMS.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Services
{
    /// <summary>
    /// Provides services for managing and querying system audit logs.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public AuditLogService(AppDbContext context) => _context = context;

        /// <summary>
        /// Asynchronously retrieves a list of audit logs based on the provided filter criteria.
        /// </summary>
        /// <param name="roleId">The optional role ID to filter logs by the user who performed the action.</param>
        /// <param name="day">The optional day of the month to filter logs by creation date.</param>
        /// <param name="month">The optional month to filter logs by creation date.</param>
        /// <returns>A task containing an enumerable collection of filtered <see cref="AuditLog"/> entities, ordered by newest first.</returns>
        /// <summary>
        /// Retrieves audit log records based on the selected filters.
        /// </summary>
        /// <param name="roleId">
        /// Optional role identifier used to filter logs by the user who performed the action.
        /// </param>
        /// <param name="day">
        /// Optional day of month.
        /// </param>
        /// <param name="month">
        /// Optional month.
        /// </param>
        /// <returns>
        /// Returns a collection of filtered audit log records.
        /// </returns>
        public async Task<IEnumerable<AuditLog>> GetFilteredLogsAsync(int? roleId, int? day, int? month)
        {
            //----------------------------------------------------------
            // Base Query
            //----------------------------------------------------------
            var query = _context.AuditLogs.Include(a => a.PerformedByAccount).ThenInclude(a => a.User).Include(a => a.PerformedByAccount).ThenInclude(a => a.Role).Include(a => a.TargetUser).AsQueryable();
            //----------------------------------------------------------
            // Filter By Role
            //----------------------------------------------------------
            if (roleId.HasValue)
            {
                query = query.Where(a => a.PerformedByAccount != null && a.PerformedByAccount.Role_Id == roleId.Value);
            }
            //----------------------------------------------------------
            // Filter By Day
            //----------------------------------------------------------
            if (day.HasValue)
            {
                query = query.Where(a => a.Created_At.Day == day.Value);
            }
            //----------------------------------------------------------
            // Filter By Month
            //----------------------------------------------------------
            if (month.HasValue)
            {
                query = query.Where(a => a.Created_At.Month == month.Value);
            }
            //----------------------------------------------------------
            // Return Result
            //----------------------------------------------------------
            return await query.OrderByDescending(a => a.Created_At).ToListAsync();
        }
    }
}