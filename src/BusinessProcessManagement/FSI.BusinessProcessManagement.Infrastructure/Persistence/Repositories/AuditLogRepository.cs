using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(BpmDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(long userId)
            => await _dbSetTEntity.AsNoTracking().Where(a => a.UserId == userId).ToListAsync();
    }
}
