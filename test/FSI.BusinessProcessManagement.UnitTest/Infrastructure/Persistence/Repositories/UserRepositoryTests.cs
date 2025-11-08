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
    public class UserRepositoryTests
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

        private static async Task<long> SeedUserAsync(BpmDbContext ctx, string username = "user1", string hash = "hash1")
        {
            var user = new User(username, hash);
            await ctx.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.Id;
        }

        private static async Task<(long userId, long adminRoleId, long editorRoleId)> SeedUserWithRolesAsync(BpmDbContext ctx)
        {
            var userId = await SeedUserAsync(ctx, "jose", "hash-jose");

            var admin = new Role("Admin", "Acesso total");
            var editor = new Role("Editor", "Pode editar");
            await ctx.AddRangeAsync(admin, editor);
            await ctx.SaveChangesAsync();

             var ur1 = new UserRole(userId, admin.Id);
            var ur2 = new UserRole(userId, editor.Id);
            await ctx.AddRangeAsync(ur1, ur2);
            await ctx.SaveChangesAsync();

            return (userId, admin.Id, editor.Id);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturn_User_WhenExists()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new UserRepository(ctx);

            await SeedUserAsync(ctx, "maria", "hash-maria");

            var found = await repo.GetByUsernameAsync("maria");

            Assert.NotNull(found);
            Assert.Equal("maria", found!.Username);

            Assert.Equal(EntityState.Detached, ctx.Entry(found).State);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturn_Null_WhenNotExists()
        {
            using var ctx = CreateInMemoryContext();
            var repo = new UserRepository(ctx);

            await SeedUserAsync(ctx, "joao", "hash-joao");

            var found = await repo.GetByUsernameAsync("inexistente");
            Assert.Null(found);
        }

        [Fact]
        public async Task GetRoleNamesAsync_ShouldReturn_UserRoleNames_Distinct()
        {
            using var ctx = CreateInMemoryContext();
            var (userId, adminRoleId, editorRoleId) = await SeedUserWithRolesAsync(ctx);

            var otherUserId = await SeedUserAsync(ctx, "outro", "hash-outro");
            var viewer = new Role("Viewer", "Somente leitura");
            await ctx.AddAsync(viewer);
            await ctx.SaveChangesAsync();
            await ctx.AddAsync(new UserRole(otherUserId, viewer.Id));
            await ctx.SaveChangesAsync();

            var repo = new UserRepository(ctx);

            var roleNames = await repo.GetRoleNamesAsync(userId);

            Assert.NotNull(roleNames);
            Assert.Equal(2, roleNames.Count);
            Assert.Contains("Admin", roleNames);
            Assert.Contains("Editor", roleNames);
            Assert.DoesNotContain("Viewer", roleNames);
        }

        [Fact]
        public async Task GetRoleNamesAsync_ShouldReturn_Empty_WhenUserHasNoRoles()
        {
            using var ctx = CreateInMemoryContext();
            var userId = await SeedUserAsync(ctx, "semroles", "hash-semroles");

            var repo = new UserRepository(ctx);
            var roleNames = await repo.GetRoleNamesAsync(userId);

            Assert.NotNull(roleNames);
            Assert.Empty(roleNames);
        }
    }
}
