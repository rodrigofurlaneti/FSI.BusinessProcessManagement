namespace FSI.BusinessProcessManagement.Domain.Interfaces
{
    public interface IProcessExecutionRepository : IRepository<Entities.ProcessExecution>
    {
        Task<IEnumerable<Entities.ProcessExecution>> GetByProcessAsync(long processId);
    }
}
