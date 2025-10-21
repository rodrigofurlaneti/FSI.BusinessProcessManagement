using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    /// <summary>
    /// Serviço genérico de aplicação para CRUD sob um repositório do domínio.
    /// Subclasses devem mapear DTO &lt;-&gt; Entidade.
    /// </summary>
    public abstract class GenericAppService<TDto, TEntity> : IAppService<TDto>
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

            // Tenta pegar a PK após insert:
            var getIdProp = entity.GetType().GetProperty("Id");
            if (getIdProp != null)
            {
                var idObj = getIdProp.GetValue(entity);
                if (idObj is long idLong) return idLong;
            }
            return 0L;
        }

        public virtual async Task UpdateAsync(TDto dto)
        {
            // Implementação padrão: mapeia DTO -> entidade “nova” e faz update direto
            // (Subclasses podem sobrescrever para atualizar campos pontualmente)
            var entity = MapToEntity(dto);
            await Repository.UpdateAsync(entity);
            await Uow.CommitAsync();
        }

        public virtual async Task DeleteAsync(long id)
        {
            await Repository.DeleteAsync(id);
            await Uow.CommitAsync();
        }
    }
}
