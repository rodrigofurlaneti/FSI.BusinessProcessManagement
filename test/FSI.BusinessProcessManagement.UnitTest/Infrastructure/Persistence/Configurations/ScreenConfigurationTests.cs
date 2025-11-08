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
    public class ScreenConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<Screen> Screens => Set<Screen>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new ScreenConfiguration());
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
            var entity = model.FindEntityType(typeof(Screen))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("Screen", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("ScreenId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_And_Types_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            var name = entity.FindProperty(nameof(Screen.Name))!;
            Assert.Equal("ScreenName", name.GetColumnName(store));
            Assert.False(name.IsNullable);
            Assert.Equal(150, name.GetMaxLength());

            var description = entity.FindProperty(nameof(Screen.Description))!;
            Assert.Equal("Description", description.GetColumnName(store));

            var createdAt = entity.FindProperty(nameof(Screen.CreatedAt))!;
            var updatedAt = entity.FindProperty(nameof(Screen.UpdatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());
        }

        [Fact]
        public void Should_Have_Unique_Index_On_Name_With_Custom_Name()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Screen.Name) }));

            Assert.NotNull(idx);
            Assert.True(idx!.IsUnique);
            Assert.Equal("UQ_Screen_Name", idx.GetDatabaseName());
        }
    }
}