using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Repositories
{
    public class ProcessRepositoryTests
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

        private static async Task<long> SeedDepartmentAsync(BpmDbContext ctx, string name)
        {
            var dept = new Department(name, $"{name} description");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        [Fact]
        public async Task GetByDepartmentAsync_ShouldReturn_OnlyProcesses_FromThatDepartment()
        {
            using var ctx = CreateInMemoryContext();
            var deptA = await SeedDepartmentAsync(ctx, "TI");
            var deptB = await SeedDepartmentAsync(ctx, "Financeiro");

            var processes = new List<Process>
            {
                new Process("Processo 1", deptA, "Fluxo TI", 1),
                new Process("Processo 2", deptA, "Controle TI", 1),
                new Process("Processo 3", deptB, "Controle Financeiro", 1)
            };

            await ctx.AddRangeAsync(processes);
            await ctx.SaveChangesAsync();

            var repo = new ProcessRepository(ctx);

            var result = await repo.GetByDepartmentAsync(deptA);
            var list = result.ToList();

            Assert.Equal(2, list.Count);
            Assert.All(list, p => Assert.Equal(deptA, p.DepartmentId));
        }

        [Fact]
        public async Task GetByDepartmentAsync_ShouldReturn_Empty_WhenNoProcessesExist()
        {
            using var ctx = CreateInMemoryContext();
            var dept = await SeedDepartmentAsync(ctx, "RH");

            var repo = new ProcessRepository(ctx);
            var result = await repo.GetByDepartmentAsync(dept);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByDepartmentAsync_ShouldNotTrack_Entities()
        {
            using var ctx = CreateInMemoryContext();
            var dept = await SeedDepartmentAsync(ctx, "Operações");

            var process = new Process("Processo Track", dept, "Fluxo Operacional", 1);
            await ctx.AddAsync(process);
            await ctx.SaveChangesAsync();

            var repo = new ProcessRepository(ctx);
            var result = await repo.GetByDepartmentAsync(dept);

            var entity = Assert.Single(result);
            Assert.Equal(EntityState.Detached, ctx.Entry(entity).State);
        }

        [Fact]
        public async Task InsertAsync_ShouldPersist_Process()
        {
            using var ctx = CreateInMemoryContext();
            var dept = await SeedDepartmentAsync(ctx, "Jurídico");

            var repo = new ProcessRepository(ctx);
            var process = new Process("Processo Jurídico", dept, "Ações legais", 1);

            await repo.InsertAsync(process);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Process>().AnyAsync(p => p.Name == "Processo Jurídico");
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_Process()
        {
            using var ctx = CreateInMemoryContext();
            var dept = await SeedDepartmentAsync(ctx, "Compras");

            var process = new Process("Processo de Compras", dept, "Fluxo de compras", 1);
            await ctx.AddAsync(process);
            await ctx.SaveChangesAsync();

            var repo = new ProcessRepository(ctx);
            await repo.DeleteAsync(process.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Process>().AnyAsync(p => p.Id == process.Id);
            Assert.False(exists);
        }
    }
}