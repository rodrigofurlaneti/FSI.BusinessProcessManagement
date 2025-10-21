using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Domain.Services
{
    public class AuditService
    {
        private readonly IUnitOfWork _uow;
        public AuditService(IUnitOfWork uow) => _uow = uow;
        public async Task<Entities.AuditLog> CreateLogAsync(string actionType, long? userId = null, long? screenId = null, string? additionalInfo = null)
        {
            if (userId.HasValue)
            {
                var user = await _uow.Users.GetByIdAsync(userId.Value);
                if (user == null) throw new InvalidOperationException("User not found.");
            }
            if (screenId.HasValue)
            {
                var screen = await _uow.Screens.GetByIdAsync(screenId.Value);
                if (screen == null) throw new InvalidOperationException("Screen not found.");
            }
            var log = new Entities.AuditLog(actionType, userId, screenId, additionalInfo);
            await _uow.AuditLogs.InsertAsync(log);
            await _uow.CommitAsync();
            return log;
        }
    }
}
