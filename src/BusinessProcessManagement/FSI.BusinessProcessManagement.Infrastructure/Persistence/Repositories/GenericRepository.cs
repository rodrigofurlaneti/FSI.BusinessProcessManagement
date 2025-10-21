using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSetTEntity;

        public GenericRepository(DbContext ctx)
        {
            _dbContext = ctx;
            _dbSetTEntity = _dbContext.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
            => await _dbSetTEntity.AsNoTracking().ToListAsync();

        public virtual async Task<TEntity?> GetByIdAsync(long id)
            => await _dbSetTEntity.FindAsync(id);

        public virtual async Task InsertAsync(TEntity entity)
            => await _dbSetTEntity.AddAsync(entity);

        public virtual Task UpdateAsync(TEntity entity)
        {
            _dbSetTEntity.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null) _dbSetTEntity.Remove(entity);
        }
    }
}
