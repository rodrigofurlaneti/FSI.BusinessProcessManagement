using FSI.BusinessProcessManagement.Domain.Exceptions;
namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region AuditLog
    public sealed class AuditLog : BaseEntity
    {
        public long? UserId { get; private set; }
        public long? ScreenId { get; private set; }
        public string ActionType { get; private set; }
        public DateTime ActionTimestamp { get; private set; }
        public string? AdditionalInfo { get; private set; }
        private AuditLog() { }
        public AuditLog(string actionType, long? userId = null, long? screenId = null, string? additionalInfo = null)
        {
            if (string.IsNullOrWhiteSpace(actionType)) throw new DomainException("ActionType is required.");
            if (actionType.Length > 60) throw new DomainException("ActionType too long (max 60).");
            ActionType = actionType.Trim();
            UserId = userId;
            ScreenId = screenId;
            AdditionalInfo = additionalInfo;
            ActionTimestamp = DateTime.UtcNow;
        }
        public void UpdateInfo(string? info)
        {
            AdditionalInfo = info;
            ActionTimestamp = DateTime.UtcNow;
            Touch();
        }
    }
    #endregion
}
