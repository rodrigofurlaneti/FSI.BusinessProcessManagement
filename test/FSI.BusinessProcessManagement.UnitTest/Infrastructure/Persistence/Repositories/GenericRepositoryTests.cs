using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Repositories
{
    public class GenericRepositoryTests
    {
        private static BpmDbContext CreateInMemoryContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<BpmDbContext>()
                .UseSqlite(connection)
                .Options;

            var ctx = new BpmDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_All_And_AsNoTracking()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new GenericRepository<Department>(ctx);

            var depts = new List<Department>
            {
                new Department("RH", "Recursos Humanos"),
                new Department("TI", "Tecnologia da Informação")
            };
            await ctx.AddRangeAsync(depts);
            await ctx.SaveChangesAsync();

            var result = await repo.GetAllAsync();
            var list = result.ToList();

            Assert.Equal(2, list.Count);
            // Verifica AsNoTracking: entidades devem estar Detached
            foreach (var d in list)
                Assert.Equal(EntityState.Detached, ctx.Entry(d).State);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_Entity_WhenExists()
        {
            using var ctx = CreateInMemoryContext();
            var dept = new Department("Financeiro", "Depto Financeiro");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var repo = new GenericRepository<Department>(ctx);
            var found = await repo.GetByIdAsync(dept.Id);

            Assert.NotNull(found);
            Assert.Equal("Financeiro", found!.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_Null_WhenNotExists()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new GenericRepository<Department>(ctx);

            var found = await repo.GetByIdAsync(999);

            Assert.Null(found);
        }

        [Fact]
        public async Task InsertAsync_ShouldAdd_And_SavePersists()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new GenericRepository<Department>(ctx);

            var dept = new Department("Comercial", "Vendas e Marketing");
            await repo.InsertAsync(dept);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Department>().AnyAsync(d => d.Id == dept.Id && d.Name == "Comercial");
            Assert.True(exists);
        }

        [Fact]
        public async Task UpdateAsync_ShouldMarkModified_And_SavePersists()
        {
            using var ctx = CreateInMemoryContext();

            var dept = new Department("Operações", "Ops");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var repo = new GenericRepository<Department>(ctx);
            dept.SetName("Operações e Logística");

            await repo.UpdateAsync(dept);
            // Verifica que EF marcou como Modified (no ChangeTracker)
            Assert.Equal(EntityState.Modified, ctx.Entry(dept).State);

            await ctx.SaveChangesAsync();

            var updated = await ctx.Set<Department>().FindAsync(dept.Id);
            Assert.Equal("Operações e Logística", updated!.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_WhenExists()
        {
            using var ctx = CreateInMemoryContext();

            var dept = new Department("Jurídico", "Depto Jurídico");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var repo = new GenericRepository<Department>(ctx);
            await repo.DeleteAsync(dept.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Department>().AnyAsync(d => d.Id == dept.Id);
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_WhenNotExists()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new GenericRepository<Department>(ctx);

            // Não deve lançar exceção ao tentar remover ID inexistente
            await repo.DeleteAsync(123456);
            await ctx.SaveChangesAsync();

            // Banco segue íntegro
            var count = await ctx.Set<Department>().CountAsync();
            Assert.Equal(0, count);
        }
    }
}