using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;
using System.Linq;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class DepartmentConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<Department> Departments => Set<Department>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
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
            var entity = model.FindEntityType(typeof(Department))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("Department", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);
            Assert.Equal("Id", pk.Properties[0].Name);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("DepartmentId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            // Name
            var name = entity.FindProperty(nameof(Department.Name))!;
            Assert.Equal("DepartmentName", name.GetColumnName(store));
            Assert.Equal(150, name.GetMaxLength());
            Assert.False(name.IsNullable);

            // Description
            var desc = entity.FindProperty(nameof(Department.Description))!;
            Assert.Equal("Description", desc.GetColumnName(store));

            // CreatedAt / UpdatedAt
            var created = entity.FindProperty(nameof(Department.CreatedAt))!;
            var updated = entity.FindProperty(nameof(Department.UpdatedAt))!;
            Assert.Equal("datetime(6)", created.GetColumnType());
            Assert.Equal("datetime(6)", updated.GetColumnType());
        }

        [Fact]
        public void Should_Have_Unique_Index_On_Name()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Department.Name)));

            Assert.NotNull(idx);
            Assert.True(idx!.IsUnique);
            Assert.Equal("UQ_Department_Name", idx.GetDatabaseName());
        }
    }
}
