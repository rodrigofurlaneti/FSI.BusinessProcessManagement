using System.Collections.Generic;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IProcessStepRepository : IRepository<ProcessStep>
    {
        Task<IEnumerable<ProcessStep>> GetByProcessIdAsync(long processId);
    }
}
