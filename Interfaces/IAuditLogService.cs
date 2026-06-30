using HRMS.Data.Entities;

namespace HRMS.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetFilteredLogsAsync(int? roleId,int? day,int? month);
        Task AddLogAsync(int performedById, object entity, string module, string action);
    }
}