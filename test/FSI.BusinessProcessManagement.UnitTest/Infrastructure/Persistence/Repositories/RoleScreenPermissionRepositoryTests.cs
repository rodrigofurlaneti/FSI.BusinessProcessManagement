using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Repositories
{
    public class RoleScreenPermissionRepositoryTests
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

        private static async Task<(long roleId, long screenId)> SeedRoleAndScreenAsync(BpmDbContext ctx)
        {
            var role = new Role("Administrador", "Acesso total");
            var screen = new Screen("Dashboard", "Tela principal");

            await ctx.AddAsync(role);
            await ctx.AddAsync(screen);
            await ctx.SaveChangesAsync();

            return (role.Id, screen.Id);
        }

        [Fact]
        public async Task GetByRoleAndScreenAsync_ShouldReturn_Entity_WhenExists()
        {
            using var ctx = CreateInMemoryContext();
            var (roleId, screenId) = await SeedRoleAndScreenAsync(ctx);

            var perm = new RoleScreenPermission(roleId, screenId, canView: true, canCreate: true, canEdit: false, canDelete: false);
            await ctx.AddAsync(perm);
            await ctx.SaveChangesAsync();

            var repo = new RoleScreenPermissionRepository(ctx);

            var found = await repo.GetByRoleAndScreenAsync(roleId, screenId);

            Assert.NotNull(found);
            Assert.Equal(roleId, found!.RoleId);
            Assert.Equal(screenId, found.ScreenId);
            Assert.True(found.CanView);
            Assert.True(found.CanCreate);
            Assert.False(found.CanEdit);
            Assert.False(found.CanDelete);
        }

        [Fact]
        public async Task GetByRoleAndScreenAsync_ShouldReturn_Null_WhenNotExists()
        {
            using var ctx = CreateInMemoryContext();
            var (roleId, screenId) = await SeedRoleAndScreenAsync(ctx);

            var repo = new RoleScreenPermissionRepository(ctx);

            var found = await repo.GetByRoleAndScreenAsync(roleId, screenId); // nada criado
            Assert.Null(found);
        }

        [Fact]
        public async Task GetByRoleAndScreenAsync_ShouldBe_AsNoTracking()
        {
            using var ctx = CreateInMemoryContext();
            var (roleId, screenId) = await SeedRoleAndScreenAsync(ctx);

            var perm = new RoleScreenPermission(roleId, screenId, true, false, false, false);
            await ctx.AddAsync(perm);
            await ctx.SaveChangesAsync();

            var repo = new RoleScreenPermissionRepository(ctx);

            var found = await repo.GetByRoleAndScreenAsync(roleId, screenId);
            Assert.NotNull(found);

            // Deve estar Detached (AsNoTracking)
            Assert.Equal(EntityState.Detached, ctx.Entry(found!).State);
        }

        [Fact]
        public async Task InsertAsync_ShouldPersist_Permission()
        {
            using var ctx = CreateInMemoryContext();
            var (roleId, screenId) = await SeedRoleAndScreenAsync(ctx);

            var repo = new RoleScreenPermissionRepository(ctx);
            var perm = new RoleScreenPermission(roleId, screenId, true, true, true, false);

            await repo.InsertAsync(perm);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<RoleScreenPermission>().AnyAsync(p => p.Id == perm.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_Permission()
        {
            using var ctx = CreateInMemoryContext();
            var (roleId, screenId) = await SeedRoleAndScreenAsync(ctx);

            var perm = new RoleScreenPermission(roleId, screenId, true, false, false, false);
            await ctx.AddAsync(perm);
            await ctx.SaveChangesAsync();

            var repo = new RoleScreenPermissionRepository(ctx);
            await repo.DeleteAsync(perm.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<RoleScreenPermission>().AnyAsync(p => p.Id == perm.Id);
            Assert.False(exists);
        }
    }
}