using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;
using System.Linq;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class ProcessConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<Process> Processes => Set<Process>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new ProcessConfiguration());
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
            var entity = model.FindEntityType(typeof(Process))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("ProcessoBPM", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("ProcessId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            // Name
            var name = entity.FindProperty(nameof(Process.Name))!;
            Assert.Equal("ProcessName", name.GetColumnName(store));
            Assert.Equal(200, name.GetMaxLength());
            Assert.False(name.IsNullable);

            // DepartmentId
            var dept = entity.FindProperty(nameof(Process.DepartmentId))!;
            Assert.Equal("DepartmentId", dept.GetColumnName(store));

            // Description
            var desc = entity.FindProperty(nameof(Process.Description))!;
            Assert.Equal("Description", desc.GetColumnName(store));

            // CreatedBy
            var createdBy = entity.FindProperty(nameof(Process.CreatedBy))!;
            Assert.Equal("CreatedBy", createdBy.GetColumnName(store));

            // CreatedAt / UpdatedAt
            var createdAt = entity.FindProperty(nameof(Process.CreatedAt))!;
            var updatedAt = entity.FindProperty(nameof(Process.UpdatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());
        }

        [Fact]
        public void Should_Have_Unique_Index_On_DepartmentId_And_Name()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { nameof(Process.DepartmentId), nameof(Process.Name) })
                );

            Assert.NotNull(idx);
            Assert.Equal("UQ_ProcessName_Department", idx!.GetDatabaseName());
        }
    }
}