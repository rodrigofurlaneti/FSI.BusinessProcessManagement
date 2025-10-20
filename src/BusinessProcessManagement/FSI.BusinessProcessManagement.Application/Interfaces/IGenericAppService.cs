namespace FSI.BusinessProcessManagement.Application.Interfaces
{
    /// <summary>
    /// Contrato genérico para AppServices que implementam CRUD básico.
    /// </summary>
    public interface IGenericAppService<TDto>
        where TDto : class
    {
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<TDto?> GetByIdAsync(long id);
        Task<long> InsertAsync(TDto dto);
        Task UpdateAsync(TDto dto);
        Task DeleteAsync(long id);
    }
}
