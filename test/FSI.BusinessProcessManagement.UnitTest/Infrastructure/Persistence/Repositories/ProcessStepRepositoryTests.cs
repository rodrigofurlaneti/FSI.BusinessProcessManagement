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
    public class ProcessStepRepositoryTests
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

        private static async Task<long> SeedProcessAsync(BpmDbContext ctx, string deptName = "TI", string processName = "Processo A")
        {
            var dept = new Department(deptName, $"{deptName} desc");
            await ctx.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var process = new Process(processName, dept.Id, $"{processName} desc", 1);
            await ctx.AddAsync(process);
            await ctx.SaveChangesAsync();

            return process.Id;
        }

        [Fact]
        public async Task GetByProcessIdAsync_ShouldReturn_OnlySteps_OrderedByStepOrder()
        {
            using var ctx = CreateInMemoryContext();

            var procA = await SeedProcessAsync(ctx, "TI", "Proc A");
            var procB = await SeedProcessAsync(ctx, "Financeiro", "Proc B");

            var steps = new List<ProcessStep>
            {
                new ProcessStep(procA, "Análise",      2, null),
                new ProcessStep(procA, "Início",       1, null),
                new ProcessStep(procA, "Conclusão",    3, null),
                new ProcessStep(procB, "Outro Proc",   1, null),
            };

            await ctx.AddRangeAsync(steps);
            await ctx.SaveChangesAsync();

            var repo = new ProcessStepRepository(ctx);

            var result = await repo.GetByProcessIdAsync(procA);
            var list = result.ToList();

            // Somente passos do processo A
            Assert.Equal(3, list.Count);
            Assert.All(list, s => Assert.Equal(procA, s.ProcessId));

            // Ordenação por StepOrder
            Assert.Collection(list,
                s => Assert.Equal(1, s.StepOrder),
                s => Assert.Equal(2, s.StepOrder),
                s => Assert.Equal(3, s.StepOrder));
        }

        [Fact]
        public async Task GetByProcessIdAsync_ShouldReturn_Empty_WhenNoSteps()
        {
            using var ctx = CreateInMemoryContext();
            var proc = await SeedProcessAsync(ctx, "RH", "Proc RH");

            var repo = new ProcessStepRepository(ctx);
            var result = await repo.GetByProcessIdAsync(proc);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByProcessIdAsync_ShouldReturn_AsNoTracking()
        {
            using var ctx = CreateInMemoryContext();
            var proc = await SeedProcessAsync(ctx, "Operações", "Proc Op");

            var step = new ProcessStep(proc, "Etapa Única", 1, null);
            await ctx.AddAsync(step);
            await ctx.SaveChangesAsync();

            var repo = new ProcessStepRepository(ctx);
            var result = await repo.GetByProcessIdAsync(proc);

            var entity = Assert.Single(result);
            Assert.Equal(EntityState.Detached, ctx.Entry(entity).State);
        }

        [Fact]
        public async Task InsertAsync_ShouldPersist_Step()
        {
            using var ctx = CreateInMemoryContext();
            var proc = await SeedProcessAsync(ctx, "Jurídico", "Proc J");

            var repo = new ProcessStepRepository(ctx);
            var step = new ProcessStep(proc, "Etapa Nova", 1, null);

            await repo.InsertAsync(step);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<ProcessStep>().AnyAsync(s => s.Id == step.Id && s.ProcessId == proc);
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_Step()
        {
            using var ctx = CreateInMemoryContext();
            var proc = await SeedProcessAsync(ctx, "Compras", "Proc C");

            var step = new ProcessStep(proc, "Apagar", 1, null);
            await ctx.AddAsync(step);
            await ctx.SaveChangesAsync();

            var repo = new ProcessStepRepository(ctx);
            await repo.DeleteAsync(step.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<ProcessStep>().AnyAsync(s => s.Id == step.Id);
            Assert.False(exists);
        }
    }
}