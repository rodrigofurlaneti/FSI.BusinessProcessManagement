using System.Collections.Generic;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IProcessRepository : IRepository<Process>
    {
        Task<IEnumerable<Process>> GetByDepartmentAsync(long departmentId);
    }
}
