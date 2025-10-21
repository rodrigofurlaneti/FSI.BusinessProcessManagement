using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class RoleScreenPermissionMapper
    {
        public static RoleScreenPermission ToNewEntity(RoleScreenPermissionDto dto)
            => new RoleScreenPermission(dto.RoleId, dto.ScreenId, dto.CanView, dto.CanCreate, dto.CanEdit, dto.CanDelete);

        public static void CopyToExisting(RoleScreenPermission entity, RoleScreenPermissionDto dto)
            => entity.SetPermissions(dto.CanView, dto.CanCreate, dto.CanEdit, dto.CanDelete);

        public static RoleScreenPermissionDto ToDto(RoleScreenPermission entity) => new()
        {
            RoleScreenPermissionId = entity.Id,
            RoleId = entity.RoleId,
            ScreenId = entity.ScreenId,
            CanView = entity.CanView,
            CanCreate = entity.CanCreate,
            CanEdit = entity.CanEdit,
            CanDelete = entity.CanDelete
        };
    }
}
