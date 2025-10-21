using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class UserRoleMapper
    {
        public static UserRole ToNewEntity(UserRoleDto dto)
            => new UserRole(dto.UserId, dto.RoleId);

        public static void CopyToExisting(UserRole entity, UserRoleDto dto)
        {
            entity.SetUser(dto.UserId);
            entity.SetRole(dto.RoleId);
        }

        public static UserRoleDto ToDto(UserRole entity) => new()
        {
            UserRoleId = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId
        };
    }
}
