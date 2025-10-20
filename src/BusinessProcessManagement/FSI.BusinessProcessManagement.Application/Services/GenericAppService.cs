using System.Collections.Generic;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public abstract class GenericAppService<TDto, TEntity> where TEntity : class
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;

        protected GenericAppService(IRepository<TEntity> repository, IUnitOfWork uow)
        {
            _repository = repository;
            _unitOfWork = uow;
        }

        protected abstract TEntity MapToEntity(TDto dto);
        protected abstract TDto MapToDto(TEntity entity);

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            var result = new List<TDto>();
            foreach (var item in list)
                result.Add(MapToDto(item));
            return result;
        }

        public virtual async Task<TDto?> GetByIdAsync(long id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? MapToDto(entity) : null;
        }

        public virtual async Task<long> InsertAsync(TDto dto)
        {
            var entity = MapToEntity(dto);
            await _repository.InsertAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            var idProp = entity.GetType().GetProperty("Id") ?? entity.GetType().GetProperty($"{typeof(TEntity).Name}Id");
            return (long)(idProp?.GetValue(entity) ?? 0);
        }

        public virtual async Task UpdateAsync(TDto dto)
        {
            var entity = MapToEntity(dto);
            await _repository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(long id)
        {
            await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
