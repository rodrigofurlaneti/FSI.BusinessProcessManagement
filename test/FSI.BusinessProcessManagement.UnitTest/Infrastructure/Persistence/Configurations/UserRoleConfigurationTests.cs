using System.Linq;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class UserRoleConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<UserRole> UserRoles => Set<UserRole>();
            public DbSet<User> Users => Set<User>();
            public DbSet<Role> Roles => Set<Role>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
                // Não é necessário aplicar configs de User/Role para inspecionar FKs
                base.OnModelCreating(modelBuilder);
            }
        }

        private static (IModel model, IEntityType urEntity) BuildModel()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options;

            using var ctx = new TestDbContext(options);
            var model = ctx.Model;
            var urEntity = model.FindEntityType(typeof(UserRole))!;
            return (model, urEntity);
        }

        [Fact]
        public void Should_Map_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("UserRole", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("UserRoleId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_And_Types()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            var userId = entity.FindProperty(nameof(UserRole.UserId))!;
            Assert.Equal("UserId", userId.GetColumnName(store));
            Assert.False(userId.IsNullable);

            var roleId = entity.FindProperty(nameof(UserRole.RoleId))!;
            Assert.Equal("RoleId", roleId.GetColumnName(store));
            Assert.False(roleId.IsNullable);

            var assignedAt = entity.FindProperty(nameof(UserRole.AssignedAt))!;
            Assert.Equal("AssignedAt", assignedAt.GetColumnName(store));
            Assert.Equal("datetime(6)", assignedAt.GetColumnType());

            var createdAt = entity.FindProperty(nameof(UserRole.CreatedAt))!;
            Assert.Equal("CreatedAt", createdAt.GetColumnName(store));
            Assert.Equal("datetime(6)", createdAt.GetColumnType());

            var updatedAt = entity.FindProperty(nameof(UserRole.UpdatedAt))!;
            Assert.Equal("UpdatedAt", updatedAt.GetColumnName(store));
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());
        }

        [Fact]
        public void Should_Have_Unique_Index_On_UserId_And_RoleId_With_Name()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                     .SequenceEqual(new[] { nameof(UserRole.UserId), nameof(UserRole.RoleId) })
                );

            Assert.NotNull(idx);
            Assert.True(idx!.IsUnique);
            Assert.Equal("UQ_UserRole_User_Role", idx.GetDatabaseName());
        }

        [Fact]
        public void Should_Have_FK_To_User_With_Cascade_And_ConstraintName()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(User));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_UserRole_User", fk.GetConstraintName());
            Assert.Equal(new[] { "UserId" }, fk.Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Should_Have_FK_To_Role_With_Cascade_And_ConstraintName()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Role));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_UserRole_Role", fk.GetConstraintName());
            Assert.Equal(new[] { "RoleId" }, fk.Properties.Select(p => p.Name).ToArray());
        }
    }
}