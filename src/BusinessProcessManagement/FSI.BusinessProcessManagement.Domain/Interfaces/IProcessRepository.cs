namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IProcessRepository : IRepository<Entities.Process>
    {
        Task<IEnumerable<Entities.Process>> GetByDepartmentAsync(long departmentId);
    }
}
