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
    public class UserConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<User> Users => Set<User>();
            public DbSet<UserRole> UserRoles => Set<UserRole>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new UserConfiguration());
                base.OnModelCreating(modelBuilder);
            }
        }

        private static (IModel model, IEntityType userEntity, IEntityType userRoleEntity) BuildModel()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options;

            using var ctx = new TestDbContext(options);
            var model = ctx.Model;
            var userEntity = model.FindEntityType(typeof(User))!;
            var userRoleEntity = model.FindEntityType(typeof(UserRole))!;
            return (model, userEntity, userRoleEntity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, user, _) = BuildModel();
            Assert.Equal("User", user.GetTableName());

            var pk = user.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(user.GetTableName()!, user.GetSchema());
            Assert.Equal("UserId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_And_Types_Correctly()
        {
            var (_, user, _) = BuildModel();
            var store = StoreObjectIdentifier.Table(user.GetTableName()!, user.GetSchema());

            var deptId = user.FindProperty(nameof(User.DepartmentId))!;
            Assert.Equal("DepartmentId", deptId.GetColumnName(store));

            var username = user.FindProperty(nameof(User.Username))!;
            Assert.Equal("Username", username.GetColumnName(store));
            Assert.False(username.IsNullable);
            Assert.Equal(100, username.GetMaxLength());

            var pwdHash = user.FindProperty(nameof(User.PasswordHash))!;
            Assert.Equal("PasswordHash", pwdHash.GetColumnName(store));
            Assert.False(pwdHash.IsNullable);
            Assert.Equal(255, pwdHash.GetMaxLength());

            var email = user.FindProperty(nameof(User.Email))!;
            Assert.Equal("Email", email.GetColumnName(store));
            Assert.Equal(200, email.GetMaxLength());

            var isActive = user.FindProperty(nameof(User.IsActive))!;
            Assert.Equal("IsActive", isActive.GetColumnName(store));
            Assert.False(isActive.IsNullable);

            var createdAt = user.FindProperty(nameof(User.CreatedAt))!;
            var updatedAt = user.FindProperty(nameof(User.UpdatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());
        }

        [Fact]
        public void Should_Configure_UserRoles_Relationship_With_Cascade_And_ConstraintName()
        {
            var (_, user, userRole) = BuildModel();

            // FK em UserRole apontando para User
            var fk = userRole.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(User));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_UserRole_User", fk.GetConstraintName());

            // A propriedade FK deve ser UserId
            var fkPropNames = fk.Properties.Select(p => p.Name).ToArray();
            Assert.Single(fkPropNames);
            Assert.Equal("UserId", fkPropNames[0]);
        }

        [Fact]
        public void Should_Set_Navigation_UserRoles_AccessMode_Field()
        {
            var (_, user, _) = BuildModel();

            // Navegação User.UserRoles
            var navigation = user.FindNavigation(nameof(User.UserRoles));
            Assert.NotNull(navigation);

            // O access mode foi configurado para Field (backing field)
            var accessMode = navigation!.GetPropertyAccessMode();
            Assert.Equal(PropertyAccessMode.Field, accessMode);
        }
    }
}