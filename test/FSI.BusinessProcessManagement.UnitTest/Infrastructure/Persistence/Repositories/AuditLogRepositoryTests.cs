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
    public class AuditLogRepositoryTests
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

        private static async Task SeedUsersAndScreensAsync(BpmDbContext ctx)
        {
            var users = new List<User>
            {
                new User("user1", "hash1"),
                new User("user2", "hash2"),
                new User("user3", "hash3")
            };

            var screens = new List<Screen>
            {
                new Screen("Home", "Home screen"),
                new Screen("Settings", "Settings page"),
                new Screen("Reports", "Reports module")
            };

            await ctx.AddRangeAsync(users);
            await ctx.AddRangeAsync(screens);
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturn_OnlyLogsFromSpecifiedUser()
        {
            using var ctx = CreateInMemoryContext();
            await SeedUsersAndScreensAsync(ctx);

            var logs = new List<AuditLog>
            {
                new AuditLog("CREATE", 1, 1, "Log 1 from user 1"),
                new AuditLog("UPDATE", 1, 2, "Log 2 from user 1"),
                new AuditLog("DELETE", 2, 3, "Log from user 2")
            };

            await ctx.AddRangeAsync(logs);
            await ctx.SaveChangesAsync();

            var repo = new AuditLogRepository(ctx);

            var result = await repo.GetByUserAsync(1);

            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, l => Assert.Equal(1, l.UserId));
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturn_Empty_WhenUserHasNoLogs()
        {
            using var ctx = CreateInMemoryContext();
            await SeedUsersAndScreensAsync(ctx);

            var logs = new List<AuditLog>
            {
                new AuditLog("VIEW", 2, 1, "Log from user 2"),
                new AuditLog("UPDATE", 3, 2, "Log from user 3")
            };

            await ctx.AddRangeAsync(logs);
            await ctx.SaveChangesAsync();

            var repo = new AuditLogRepository(ctx);
            var result = await repo.GetByUserAsync(99);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldNotTrack_Entities()
        {
            using var ctx = CreateInMemoryContext();
            await SeedUsersAndScreensAsync(ctx);

            var log = new AuditLog("INSERT", 3, 2, "tracking test");
            await ctx.AddAsync(log);
            await ctx.SaveChangesAsync();

            var repo = new AuditLogRepository(ctx);
            var result = await repo.GetByUserAsync(3);

            var entity = Assert.Single(result);
            var entry = ctx.Entry(entity);
            Assert.Equal(EntityState.Detached, entry.State);
        }
    }
}
