using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public abstract class GenericAppService<TDto, TEntity> : IGenericAppService<TDto>
        where TDto : class
        where TEntity : class
    {
        protected readonly IUnitOfWork Uow;
        protected readonly IRepository<TEntity> Repository;

        protected GenericAppService(IUnitOfWork uow, IRepository<TEntity> repository)
        {
            Uow = uow;
            Repository = repository;
        }

        protected abstract TDto MapToDto(TEntity entity);
        protected abstract TEntity MapToEntity(TDto dto);

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var all = await Repository.GetAllAsync();
            return all.Select(MapToDto).ToList();
        }

        public virtual async Task<TDto?> GetByIdAsync(long id)
        {
            var entity = await Repository.GetByIdAsync(id);
            return entity is null ? default : MapToDto(entity);
        }

        public virtual async Task<long> InsertAsync(TDto dto)
        {
            var entity = MapToEntity(dto);
            await Repository.InsertAsync(entity);
            await Uow.CommitAsync();
            var idProp = entity.GetType().GetProperty("Id");
            return idProp?.GetValue(entity) is long id ? id : 0L;
        }

        public abstract Task UpdateAsync(TDto dto);

        public virtual async Task DeleteAsync(long id)
        {
            await Repository.DeleteAsync(id);
            await Uow.CommitAsync();
        }
    }
}
