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
            => await _dbSetTEntity.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

        public async Task<IReadOnlyList<string>> GetRoleNamesAsync(long userId)
        {
            return await _dbContext.Set<UserRole>()
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.Set<Role>(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .Distinct()
                .ToListAsync();
        }

    }
}
