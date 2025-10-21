using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class ProcessRepository : GenericRepository<DomainProcess>, IProcessRepository
    {
        private readonly BpmDbContext __dbContext;

        public ProcessRepository(BpmDbContext ctx) : base(ctx)
        {
            __dbContext = ctx;
        }

        public async Task<IEnumerable<DomainProcess>> GetByDepartmentAsync(long departmentId)
        {
            return await __dbContext.Set<DomainProcess>()
                .AsNoTracking()
                .Where(p => p.DepartmentId == departmentId)
                .ToListAsync();
        }
    }
}
