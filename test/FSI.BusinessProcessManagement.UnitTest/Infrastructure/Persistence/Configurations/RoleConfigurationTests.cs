using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class RoleConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<Role> Roles => Set<Role>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new RoleConfiguration());
                base.OnModelCreating(modelBuilder);
            }
        }

        private static (IModel model, IEntityType entity) BuildModel()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options;

            using var ctx = new TestDbContext(options);
            var model = ctx.Model;
            var entity = model.FindEntityType(typeof(Role))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("Role", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("RoleId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_And_Types_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            var name = entity.FindProperty(nameof(Role.Name))!;
            Assert.Equal("RoleName", name.GetColumnName(store));
            Assert.False(name.IsNullable);
            Assert.Equal(100, name.GetMaxLength());

            var desc = entity.FindProperty(nameof(Role.Description))!;
            Assert.Equal("Description", desc.GetColumnName(store));

            var createdAt = entity.FindProperty(nameof(Role.CreatedAt))!;
            var updatedAt = entity.FindProperty(nameof(Role.UpdatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());
        }
    }
}