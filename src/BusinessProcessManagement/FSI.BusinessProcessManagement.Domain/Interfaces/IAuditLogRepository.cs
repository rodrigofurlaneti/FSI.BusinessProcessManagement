namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IAuditLogRepository : IRepository<Entities.AuditLog>
    {
        Task<IEnumerable<Entities.AuditLog>> GetByUserAsync(long userId);
    }
}
