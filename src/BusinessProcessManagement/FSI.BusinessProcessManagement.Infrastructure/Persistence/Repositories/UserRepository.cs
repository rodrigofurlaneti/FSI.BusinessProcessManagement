using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(BpmDbContext ctx) : base(ctx) { }

        public async Task<User?> GetByUsernameAsync(string username)
            => await _dbSet.AsNoTracking()
                           .FirstOrDefaultAsync(u => u.Username == username);

        public async Task<IReadOnlyList<string>> GetRoleNamesAsync(long userId)
        {
            return await _ctx.Set<UserRole>()
                             .AsNoTracking()
                             .Where(ur => ur.UserId == userId)
                             .Join(_ctx.Set<Role>().AsNoTracking(),
                                   ur => ur.RoleId,
                                   r => r.Id,
                                   (ur, r) => r.Name)
                             .Distinct()
                             .ToListAsync();
        }
    }
}
