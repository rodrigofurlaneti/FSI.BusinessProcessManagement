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
    public class ProcessExecutionRepositoryTests
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

        private static async Task<(long processId, long step1Id, long step2Id, long userId)> SeedDependenciesAsync(BpmDbContext ctx)
        {
            var dept = new Department("TI", "Tecnologia");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var process = new Process("Processo Principal", dept.Id, "Processo de teste", 1);
            await ctx.AddAsync(process);
            await ctx.SaveChangesAsync();

            var step1 = new ProcessStep(process.Id, "Etapa 1", 1, null);
            var step2 = new ProcessStep(process.Id, "Etapa 2", 2, null);
            await ctx.AddRangeAsync(step1, step2);
            await ctx.SaveChangesAsync();

            var user = new User("usuario1", "hash123");
            await ctx.AddAsync(user);
            await ctx.SaveChangesAsync();

            return (process.Id, step1.Id, step2.Id, user.Id);
        }

        [Fact]
        public async Task GetByProcessAsync_ShouldReturn_Empty_WhenNoExecutionsExist()
        {
            using var ctx = CreateInMemoryContext();
            var seed = await SeedDependenciesAsync(ctx);

            var repo = new ProcessExecutionRepository(ctx);
            var result = await repo.GetByProcessAsync(123456);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByProcessAsync_ShouldReturn_OnlyExecutions_FromSpecifiedProcess()
        {
            using var ctx = CreateInMemoryContext();

            var dept = new Department("TI", "Tecnologia");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var processA = new Process("Processo A", dept.Id, "Proc A", 1);
            await ctx.AddAsync(processA);
            await ctx.SaveChangesAsync();

            var stepA1 = new ProcessStep(processA.Id, "Etapa A1", 1, null);
            var stepA2 = new ProcessStep(processA.Id, "Etapa A2", 2, null);
            await ctx.AddRangeAsync(stepA1, stepA2);
            await ctx.SaveChangesAsync();

            var processB = new Process("Processo B", dept.Id, "Proc B", 1);
            await ctx.AddAsync(processB);
            await ctx.SaveChangesAsync();

            var stepB1 = new ProcessStep(processB.Id, "Etapa B1", 1, null);
            await ctx.AddAsync(stepB1);
            await ctx.SaveChangesAsync();

            var user = new User("usuario1", "hash123");
            await ctx.AddAsync(user);
            await ctx.SaveChangesAsync();

            var execs = new List<ProcessExecution>
            {
                new ProcessExecution(processA.Id, stepA1.Id, user.Id),
                new ProcessExecution(processA.Id, stepA2.Id, user.Id),
                new ProcessExecution(processB.Id, stepB1.Id, user.Id)
            };

            await ctx.AddRangeAsync(execs);
            await ctx.SaveChangesAsync();

            var repo = new ProcessExecutionRepository(ctx);

            var result = await repo.GetByProcessAsync(processA.Id);
            var list = result.ToList();

            Assert.Equal(2, list.Count);
            Assert.All(list, e => Assert.Equal(processA.Id, e.ProcessId));
        }

        [Fact]
        public async Task GetByProcessAsync_ShouldNotTrack_Entities()
        {
            using var ctx = CreateInMemoryContext();
            var seed = await SeedDependenciesAsync(ctx);

            var exec = new ProcessExecution(seed.processId, seed.step1Id, seed.userId);
            await ctx.AddAsync(exec);
            await ctx.SaveChangesAsync();

            var repo = new ProcessExecutionRepository(ctx);
            var result = await repo.GetByProcessAsync(seed.processId);

            var entity = Assert.Single(result);
            Assert.Equal(EntityState.Detached, ctx.Entry(entity).State);
        }

        [Fact]
        public async Task InsertAsync_ShouldPersist_NewExecution()
        {
            using var ctx = CreateInMemoryContext();
            var seed = await SeedDependenciesAsync(ctx);

            var repo = new ProcessExecutionRepository(ctx);
            var exec = new ProcessExecution(seed.processId, seed.step1Id, seed.userId);

            await repo.InsertAsync(exec);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<ProcessExecution>().AnyAsync(e => e.Id == exec.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_ExistingExecution()
        {
            using var ctx = CreateInMemoryContext();
            var seed = await SeedDependenciesAsync(ctx);

            var exec = new ProcessExecution(seed.processId, seed.step1Id, seed.userId);
            await ctx.AddAsync(exec);
            await ctx.SaveChangesAsync();

            var repo = new ProcessExecutionRepository(ctx);
            await repo.DeleteAsync(exec.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<ProcessExecution>().AnyAsync(e => e.Id == exec.Id);
            Assert.False(exists);
        }
    }
}
