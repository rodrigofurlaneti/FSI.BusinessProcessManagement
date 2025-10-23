using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly BpmDbContext _bpmDbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(BpmDbContext bpmDbContext)
        {
            _bpmDbContext = bpmDbContext;
            _dbSet = _bpmDbContext.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
            => await _dbSet.AsNoTracking().ToListAsync();

        public virtual async Task<TEntity?> GetByIdAsync(long id)
            => await _dbSet.FindAsync(id);

        public virtual async Task InsertAsync(TEntity entity)
            => await _dbSet.AddAsync(entity);

        public virtual Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }
    }
}
