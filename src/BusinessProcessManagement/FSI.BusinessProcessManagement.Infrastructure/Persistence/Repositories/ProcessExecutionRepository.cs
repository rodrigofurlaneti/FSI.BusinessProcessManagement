using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class ProcessExecutionRepository : GenericRepository<ProcessExecution>, IProcessExecutionRepository
    {
        public ProcessExecutionRepository(BpmDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<ProcessExecution>> GetByProcessAsync(long processId)
            => await _dbSet.AsNoTracking()
                           .Where(e => e.ProcessId == processId)
                           .ToListAsync();
    }
}
