namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IProcessStepRepository : IRepository<Entities.ProcessStep>
    {
        Task<IEnumerable<Entities.ProcessStep>> GetByProcessIdAsync(long processId);
    }
}
