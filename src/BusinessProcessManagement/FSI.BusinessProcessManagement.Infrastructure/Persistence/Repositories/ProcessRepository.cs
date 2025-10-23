using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class ProcessRepository : GenericRepository<DomainProcess>, IProcessRepository
    {
        public ProcessRepository(BpmDbContext bpmDbContext) : base(bpmDbContext) { }

        public async Task<IEnumerable<DomainProcess>> GetByDepartmentAsync(long departmentId)
            => await _dbSet.AsNoTracking()
                           .Where(p => p.DepartmentId == departmentId)
                           .ToListAsync();
    }
}
