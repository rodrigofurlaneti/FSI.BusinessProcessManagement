using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class ProcessStepRepository : GenericRepository<ProcessStep>, IProcessStepRepository
    {
        public ProcessStepRepository(BpmDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<ProcessStep>> GetByProcessIdAsync(long processId)
            => await _dbSet.AsNoTracking()
                           .Where(s => s.ProcessId == processId)
                           .OrderBy(s => s.StepOrder)
                           .ToListAsync();
    }
}
