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
    public class DepartmentRepositoryTests
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
        public async Task InsertAsync_ShouldPersist_Department()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new DepartmentRepository(ctx);

            var dept = new Department("TI", "Tecnologia da Informação");
            await repo.InsertAsync(dept);
            await ctx.SaveChangesAsync();

            var dbDept = await ctx.Set<Department>().FirstOrDefaultAsync();
            Assert.NotNull(dbDept);
            Assert.Equal("TI", dbDept!.Name);
            Assert.Equal("Tecnologia da Informação", dbDept.Description);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_AllDepartments()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new DepartmentRepository(ctx);

            var depts = new List<Department>
            {
                new Department("RH", "Recursos Humanos"),
                new Department("Financeiro", "Departamento financeiro")
            };

            await ctx.AddRangeAsync(depts);
            await ctx.SaveChangesAsync();

            var result = await repo.GetAllAsync();
            var list = result.ToList();

            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.Name == "RH");
            Assert.Contains(list, d => d.Name == "Financeiro");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_CorrectEntity()
        {
            using var ctx = CreateInMemoryContext();
            var dept = new Department("Comercial", "Vendas e marketing");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var repo = new DepartmentRepository(ctx);
            var result = await repo.GetByIdAsync(dept.Id);

            Assert.NotNull(result);
            Assert.Equal("Comercial", result!.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModify_ExistingEntity()
        {
            using var ctx = CreateInMemoryContext();
            var dept = new Department("Operações", "Controle de operações");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var repo = new DepartmentRepository(ctx);
            dept.SetName("Operações Aeroportuárias");
            await repo.UpdateAsync(dept);
            await ctx.SaveChangesAsync();

            var updated = await ctx.Set<Department>().FindAsync(dept.Id);
            Assert.Equal("Operações Aeroportuárias", updated!.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_Entity()
        {
            using var ctx = CreateInMemoryContext();
            var dept = new Department("Jurídico", "Departamento jurídico");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var repo = new DepartmentRepository(ctx);
            await repo.DeleteAsync(dept.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Department>().AnyAsync(d => d.Id == dept.Id);
            Assert.False(exists);
        }
    }
}
