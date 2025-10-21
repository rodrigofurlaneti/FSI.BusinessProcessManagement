using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class AuditLogMapper
    {
        public static AuditLog ToNewEntity(AuditLogDto dto)
        {
            var e = new AuditLog(dto.ActionType ?? string.Empty, dto.UserId, dto.ScreenId);
            if (!string.IsNullOrWhiteSpace(dto.AdditionalInfo))
                e.SetAdditionalInfo(dto.AdditionalInfo);
            return e;
        }

        public static void CopyToExisting(AuditLog entity, AuditLogDto dto)
        {
            entity.SetUser(dto.UserId);
            entity.SetScreen(dto.ScreenId);
            entity.SetActionType(dto.ActionType ?? string.Empty);
            entity.SetAdditionalInfo(dto.AdditionalInfo);
        }

        public static AuditLogDto ToDto(AuditLog entity) => new()
        {
            AuditId = entity.Id,
            UserId = entity.UserId,
            ScreenId = entity.ScreenId,
            ActionType = entity.ActionType,
            ActionTimestamp = entity.ActionTimestamp,
            AdditionalInfo = entity.AdditionalInfo
        };
    }
}
