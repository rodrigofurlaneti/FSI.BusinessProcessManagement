namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IRoleScreenPermissionRepository : IRepository<Entities.RoleScreenPermission>
    {
        Task<Entities.RoleScreenPermission?> GetByRoleAndScreenAsync(long roleId, long screenId);
    }
}
