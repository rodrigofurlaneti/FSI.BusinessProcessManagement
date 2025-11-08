using System.Linq;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class RoleScreenPermissionConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<RoleScreenPermission> RoleScreenPermissions => Set<RoleScreenPermission>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new RoleScreenPermissionConfiguration());
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
            var entity = model.FindEntityType(typeof(RoleScreenPermission))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("RoleScreenPermission", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("RoleScreenPermissionId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_And_Types_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            var roleId = entity.FindProperty(nameof(RoleScreenPermission.RoleId))!;
            Assert.Equal("RoleId", roleId.GetColumnName(store));
            Assert.False(roleId.IsNullable);

            var screenId = entity.FindProperty(nameof(RoleScreenPermission.ScreenId))!;
            Assert.Equal("ScreenId", screenId.GetColumnName(store));
            Assert.False(screenId.IsNullable);

            var canView = entity.FindProperty(nameof(RoleScreenPermission.CanView))!;
            Assert.Equal("CanView", canView.GetColumnName(store));
            Assert.False(canView.IsNullable);
            Assert.Equal("tinyint(1)", canView.GetColumnType());

            var canCreate = entity.FindProperty(nameof(RoleScreenPermission.CanCreate))!;
            Assert.Equal("CanCreate", canCreate.GetColumnName(store));
            Assert.False(canCreate.IsNullable);
            Assert.Equal("tinyint(1)", canCreate.GetColumnType());

            var canEdit = entity.FindProperty(nameof(RoleScreenPermission.CanEdit))!;
            Assert.Equal("CanEdit", canEdit.GetColumnName(store));
            Assert.False(canEdit.IsNullable);
            Assert.Equal("tinyint(1)", canEdit.GetColumnType());

            var canDelete = entity.FindProperty(nameof(RoleScreenPermission.CanDelete))!;
            Assert.Equal("CanDelete", canDelete.GetColumnName(store));
            Assert.False(canDelete.IsNullable);
            Assert.Equal("tinyint(1)", canDelete.GetColumnType());
        }

        [Fact]
        public void Should_Have_Unique_Index_On_RoleId_And_ScreenId()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                     .SequenceEqual(new[] { nameof(RoleScreenPermission.RoleId), nameof(RoleScreenPermission.ScreenId) })
                );

            Assert.NotNull(idx);
            Assert.True(idx!.IsUnique);
        }

        [Fact]
        public void Should_Have_FK_To_Role_With_Cascade_And_ConstraintName()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Role));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_RoleScreenPermission_Role", fk.GetConstraintName());
        }

        [Fact]
        public void Should_Have_FK_To_Screen_With_Cascade_And_ConstraintName()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Screen));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_RoleScreenPermission_Screen", fk.GetConstraintName());
        }
    }
}