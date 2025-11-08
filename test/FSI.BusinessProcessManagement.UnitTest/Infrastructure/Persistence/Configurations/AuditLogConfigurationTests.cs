using System.Linq;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfigurationTests
    {
        // DbContext de teste aplicando a configuração
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
                base.OnModelCreating(modelBuilder);
            }
        }

        /// <summary>
        /// Constrói o modelo com provider relacional (SQLite in-memory) para habilitar metadados relacionais.
        /// </summary>
        private static (IModel model, IEntityType entity) BuildModel()
        {
            // Conexão in-memory precisa estar aberta enquanto o contexto é criado
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection) // provider relacional
                .Options;

            using var ctx = new TestDbContext(options);
            // Não precisamos EnsureCreated para inspecionar metamodelo
            var model = ctx.Model;
            var entity = model.FindEntityType(typeof(AuditLog))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.NotNull(entity);

            // Tabela
            Assert.Equal("AuditLog", entity.GetTableName());

            // PK e nome da coluna
            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var keyProp = pk.Properties[0];
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("AuditId", keyProp.GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_And_Types_And_Requirements()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            // CreatedAt / UpdatedAt -> datetime(6)
            var createdAt = entity.FindProperty(nameof(AuditLog.CreatedAt))!;
            var updatedAt = entity.FindProperty(nameof(AuditLog.UpdatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());

            // UserId / ScreenId -> nomes de coluna
            var userId = entity.FindProperty(nameof(AuditLog.UserId))!;
            var screenId = entity.FindProperty(nameof(AuditLog.ScreenId))!;
            Assert.Equal("UserId", userId.GetColumnName(store));
            Assert.Equal("ScreenId", screenId.GetColumnName(store));

            // ActionType -> Required + MaxLength(60)
            var actionType = entity.FindProperty(nameof(AuditLog.ActionType))!;
            Assert.False(actionType.IsNullable);
            Assert.Equal(60, actionType.GetMaxLength());

            // ActionTimestamp -> datetime(6)
            var actionTs = entity.FindProperty(nameof(AuditLog.ActionTimestamp))!;
            Assert.Equal("datetime(6)", actionTs.GetColumnType());

            // AdditionalInfo -> nome da coluna
            var addInfo = entity.FindProperty(nameof(AuditLog.AdditionalInfo))!;
            Assert.Equal("AdditionalInfo", addInfo.GetColumnName(store));
        }

        [Fact]
        public void Should_Have_Composite_Index_On_UserId_And_ActionTimestamp_With_Name()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                     .SequenceEqual(new[] { nameof(AuditLog.UserId), nameof(AuditLog.ActionTimestamp) })
                );

            Assert.NotNull(idx);
            Assert.Equal("IX_Audit_User_Time", idx!.GetDatabaseName());
        }

        [Fact]
        public void Should_Map_FK_To_User_With_SetNull_And_ConstraintName()
        {
            var (_, entity) = BuildModel();

            var fkToUser = entity.GetForeignKeys()
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(User));

            Assert.NotNull(fkToUser);
            Assert.Equal(DeleteBehavior.SetNull, fkToUser!.DeleteBehavior);
            Assert.Equal("FK_AuditLog_User", fkToUser.GetConstraintName());
        }

        [Fact]
        public void Should_Map_FK_To_Screen_With_SetNull_And_ConstraintName()
        {
            var (_, entity) = BuildModel();

            var fkToScreen = entity.GetForeignKeys()
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Screen));

            Assert.NotNull(fkToScreen);
            Assert.Equal(DeleteBehavior.SetNull, fkToScreen!.DeleteBehavior);
            Assert.Equal("FK_AuditLog_Screen", fkToScreen.GetConstraintName());
        }
    }
}
