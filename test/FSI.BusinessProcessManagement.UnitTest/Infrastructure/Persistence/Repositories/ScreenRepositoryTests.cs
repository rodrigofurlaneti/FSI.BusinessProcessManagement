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
    public class ScreenRepositoryTests
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
        public async Task InsertAsync_ShouldPersist_NewScreen()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new ScreenRepository(ctx);

            var screen = new Screen("Dashboard", "Tela inicial do sistema");

            await repo.InsertAsync(screen);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Screen>().AnyAsync(s => s.Id == screen.Id && s.Name == "Dashboard");
            Assert.True(exists);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_AllScreens()
        {
            using var ctx = CreateInMemoryContext();

            await ctx.AddRangeAsync(
                new Screen("Relatórios", "Listagem de relatórios"),
                new Screen("Configurações", "Preferências do sistema"),
                new Screen("Usuários", "Gestão de usuários")
            );
            await ctx.SaveChangesAsync();

            var repo = new ScreenRepository(ctx);
            var result = await repo.GetAllAsync();

            Assert.Equal(3, result.Count());
            Assert.Contains(result, s => s.Name == "Relatórios");
            Assert.Contains(result, s => s.Name == "Configurações");
            Assert.Contains(result, s => s.Name == "Usuários");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_CorrectScreen()
        {
            using var ctx = CreateInMemoryContext();
            var screen = new Screen("Auditoria", "Tela de logs");
            await ctx.AddAsync(screen);
            await ctx.SaveChangesAsync();

            var repo = new ScreenRepository(ctx);
            var fetched = await repo.GetByIdAsync(screen.Id);

            Assert.NotNull(fetched);
            Assert.Equal(screen.Id, fetched!.Id);
            Assert.Equal("Auditoria", fetched.Name);
            Assert.Equal("Tela de logs", fetched.Description);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModify_ScreenDescription()
        {
            using var ctx = CreateInMemoryContext();
            var screen = new Screen("Permissões", "Descrição antiga");
            await ctx.AddAsync(screen);
            await ctx.SaveChangesAsync();

            var repo = new ScreenRepository(ctx);

            screen.SetDescription("Descrição atualizada");
            await repo.UpdateAsync(screen);
            await ctx.SaveChangesAsync();

            var updated = await ctx.Set<Screen>().FirstAsync(s => s.Id == screen.Id);
            Assert.Equal("Descrição atualizada", updated.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_Screen()
        {
            using var ctx = CreateInMemoryContext();
            var screen = new Screen("Excluir", "Será removida");
            await ctx.AddAsync(screen);
            await ctx.SaveChangesAsync();

            var repo = new ScreenRepository(ctx);
            await repo.DeleteAsync(screen.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Screen>().AnyAsync(s => s.Id == screen.Id);
            Assert.False(exists);
        }
    }
}
