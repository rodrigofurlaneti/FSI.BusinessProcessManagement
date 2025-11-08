using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Repositories
{
    public class RoleRepositoryTests
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
        public async Task InsertAsync_ShouldPersist_NewRole()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new RoleRepository(ctx);

            var role = new Role("Administrador", "Acesso total ao sistema");

            await repo.InsertAsync(role);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Role>().AnyAsync(r => r.Id == role.Id && r.Name == "Administrador");
            Assert.True(exists);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturn_AllRoles()
        {
            using var ctx = CreateInMemoryContext();
            var roles = new[]
            {
                new Role("Gestor", "Pode aprovar processos"),
                new Role("Analista", "Pode editar e visualizar dados"),
                new Role("Leitor", "Apenas leitura")
            };

            await ctx.AddRangeAsync(roles);
            await ctx.SaveChangesAsync();

            var repo = new RoleRepository(ctx);
            var result = await repo.GetAllAsync();

            Assert.Equal(3, await ctx.Set<Role>().CountAsync());
            Assert.Collection(result,
                r => Assert.Equal("Gestor", r.Name),
                r => Assert.Equal("Analista", r.Name),
                r => Assert.Equal("Leitor", r.Name));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_CorrectRole()
        {
            using var ctx = CreateInMemoryContext();
            var role = new Role("Supervisor", "Pode monitorar atividades");
            await ctx.AddAsync(role);
            await ctx.SaveChangesAsync();

            var repo = new RoleRepository(ctx);
            var fetched = await repo.GetByIdAsync(role.Id);

            Assert.NotNull(fetched);
            Assert.Equal(role.Id, fetched!.Id);
            Assert.Equal("Supervisor", fetched.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModify_RoleDescription()
        {
            using var ctx = CreateInMemoryContext();
            var role = new Role("Auditor", "Acesso limitado");
            await ctx.AddAsync(role);
            await ctx.SaveChangesAsync();

            var repo = new RoleRepository(ctx);
            role.SetDescription("Pode revisar logs de auditoria");
            await repo.UpdateAsync(role);
            await ctx.SaveChangesAsync();

            var updated = await ctx.Set<Role>().FirstAsync(r => r.Id == role.Id);
            Assert.Equal("Pode revisar logs de auditoria", updated.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_Role()
        {
            using var ctx = CreateInMemoryContext();
            var role = new Role("Exclusão", "Será removido");
            await ctx.AddAsync(role);
            await ctx.SaveChangesAsync();

            var repo = new RoleRepository(ctx);
            await repo.DeleteAsync(role.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<Role>().AnyAsync(r => r.Id == role.Id);
            Assert.False(exists);
        }
    }
}
