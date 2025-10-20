using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Domain.ValueObjects
{
    public class PermissionService
    {
        private readonly IUnitOfWork _uow;
        public PermissionService(IUnitOfWork uow) => _uow = uow;


        public async Task AssignPermissionAsync(long roleId, long screenId, bool canView, bool canCreate, bool canEdit, bool canDelete)
        {
            var role = await _uow.Roles.GetByIdAsync(roleId) ?? throw new InvalidOperationException("Role not found.");
            var screen = await _uow.Screens.GetByIdAsync(screenId) ?? throw new InvalidOperationException("Screen not found.");
            var existing = await _uow.RoleScreenPermissions.GetByRoleAndScreenAsync(roleId, screenId);
            if (existing != null)
            {
                existing.SetPermissions(canView, canCreate, canEdit, canDelete);
                await _uow.RoleScreenPermissions.UpdateAsync(existing);
            }
            else
            {
                var rsp = new Entities.RoleScreenPermission(roleId, screenId, canView, canCreate, canEdit, canDelete);
                await _uow.RoleScreenPermissions.InsertAsync(rsp);
            }
            await _uow.CommitAsync();
        }
    }
}
