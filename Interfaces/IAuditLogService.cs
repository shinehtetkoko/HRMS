using HRMS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetFilteredLogsAsync(int? roleId, int? day, int? month);
    }
}