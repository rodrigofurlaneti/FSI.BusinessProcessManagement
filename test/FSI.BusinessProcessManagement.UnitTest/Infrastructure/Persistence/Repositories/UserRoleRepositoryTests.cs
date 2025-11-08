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
    public class UserRoleRepositoryTests
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

        private static async Task<(long userId, long roleId)> SeedUserAndRoleAsync(BpmDbContext ctx)
        {
            var user = new User("usuario1", "hash123");
            var role = new Role("Admin", "Acesso total");

            await ctx.AddRangeAsync(user, role);
            await ctx.SaveChangesAsync();

            return (user.Id, role.Id);
        }

        [Fact]
        public async Task InsertAsync_ShouldPersist_NewUserRole()
        {
            using var ctx = CreateInMemoryContext();
            var (userId, roleId) = await SeedUserAndRoleAsync(ctx);

            var repo = new UserRoleRepository(ctx);
            var userRole = new UserRole(userId, roleId);

            await repo.InsertAsync(userRole);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<UserRole>().AnyAsync(ur => ur.Id == userRole.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_CorrectUserRole()
        {
            using var ctx = CreateInMemoryContext();
            var (userId, roleId) = await SeedUserAndRoleAsync(ctx);

            var ur = new UserRole(userId, roleId);
            await ctx.AddAsync(ur);
            await ctx.SaveChangesAsync();

            var repo = new UserRoleRepository(ctx);
            var found = await repo.GetByIdAsync(ur.Id);

            Assert.NotNull(found);
            Assert.Equal(userId, found!.UserId);
            Assert.Equal(roleId, found.RoleId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_UserRole()
        {
            using var ctx = CreateInMemoryContext();
            var (userId, roleId) = await SeedUserAndRoleAsync(ctx);

            var ur = new UserRole(userId, roleId);
            await ctx.AddAsync(ur);
            await ctx.SaveChangesAsync();

            var repo = new UserRoleRepository(ctx);
            await repo.DeleteAsync(ur.Id);
            await ctx.SaveChangesAsync();

            var exists = await ctx.Set<UserRole>().AnyAsync(x => x.Id == ur.Id);
            Assert.False(exists);
        }
    }
}
