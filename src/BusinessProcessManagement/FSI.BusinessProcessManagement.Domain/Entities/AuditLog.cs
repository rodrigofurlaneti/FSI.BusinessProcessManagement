using System;
using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class AuditLog : BaseEntity
    {
        public long? UserId { get; private set; }
        public long? ScreenId { get; private set; }
        public string ActionType { get; private set; } = string.Empty;
        public DateTime ActionTimestamp { get; private set; }
        public string? AdditionalInfo { get; private set; }

        private AuditLog() { }

        // Construtor usado pela Application (actionType, userId, screenId)
        public AuditLog(string actionType, long? userId = null, long? screenId = null, string? additionalInfo = null)
        {
            SetActionType(actionType);
            SetUser(userId);
            SetScreen(screenId);
            SetAdditionalInfo(additionalInfo);
            ActionTimestamp = DateTime.UtcNow;
        }

        // ===== Métodos esperados pelos AppServices =====
        public void SetUser(long? userId)
        {
            if (userId.HasValue && userId.Value <= 0)
                throw new DomainException("Invalid UserId.");
            UserId = userId;
            Touch();
        }

        public void SetScreen(long? screenId)
        {
            if (screenId.HasValue && screenId.Value <= 0)
                throw new DomainException("Invalid ScreenId.");
            ScreenId = screenId;
            Touch();
        }

        public void SetActionType(string actionType)
        {
            if (string.IsNullOrWhiteSpace(actionType))
                throw new DomainException("ActionType is required.");
            if (actionType.Length > 60)
                throw new DomainException("ActionType too long (max 60).");

            ActionType = actionType.Trim();
            ActionTimestamp = DateTime.UtcNow;
            Touch();
        }

        public void SetAdditionalInfo(string? info)
        {
            AdditionalInfo = string.IsNullOrWhiteSpace(info) ? null : info.Trim();
            ActionTimestamp = DateTime.UtcNow;
            Touch();
        }

        // Método que você já tinha — mantido para compatibilidade
        public void UpdateInfo(string? info)
        {
            SetAdditionalInfo(info);
        }
    }
}
