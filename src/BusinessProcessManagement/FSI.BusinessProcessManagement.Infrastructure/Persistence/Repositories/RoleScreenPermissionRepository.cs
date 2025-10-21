using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class RoleScreenPermissionRepository : GenericRepository<RoleScreenPermission>, IRoleScreenPermissionRepository
    {
        public RoleScreenPermissionRepository(BpmDbContext ctx) : base(ctx) { }

        public async Task<RoleScreenPermission?> GetByRoleAndScreenAsync(long roleId, long screenId)
            => await _dbSetTEntity.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == roleId && r.ScreenId == screenId);
    }
}
