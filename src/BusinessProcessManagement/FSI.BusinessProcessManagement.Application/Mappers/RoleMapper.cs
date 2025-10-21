using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class RoleMapper
    {
        public static Role ToNewEntity(RoleDto dto)
            => new Role(dto.RoleName ?? string.Empty, dto.Description);

        public static void CopyToExisting(Role entity, RoleDto dto)
        {
            entity.SetName(dto.RoleName ?? string.Empty);
            entity.SetDescription(dto.Description);
        }

        public static RoleDto ToDto(Role entity) => new()
        {
            RoleId = entity.Id,
            RoleName = entity.Name,
            Description = entity.Description
        };
    }
}
