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
    public class ProcessStepConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new ProcessStepConfiguration());
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
            var entity = model.FindEntityType(typeof(ProcessStep))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("ProcessStep", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("StepId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            var processId = entity.FindProperty(nameof(ProcessStep.ProcessId))!;
            Assert.Equal("ProcessId", processId.GetColumnName(store));
            Assert.False(processId.IsNullable);

            var stepName = entity.FindProperty(nameof(ProcessStep.StepName))!;
            Assert.Equal("StepName", stepName.GetColumnName(store));
            Assert.False(stepName.IsNullable);
            Assert.Equal(200, stepName.GetMaxLength());

            var stepOrder = entity.FindProperty(nameof(ProcessStep.StepOrder))!;
            Assert.Equal("StepOrder", stepOrder.GetColumnName(store));
            Assert.False(stepOrder.IsNullable);

            var assignedRoleId = entity.FindProperty(nameof(ProcessStep.AssignedRoleId))!;
            Assert.Equal("AssignedRoleId", assignedRoleId.GetColumnName(store));
            // AssignedRoleId é opcional (nullable)
            Assert.True(assignedRoleId.IsNullable);

            var createdAt = entity.FindProperty(nameof(ProcessStep.CreatedAt))!;
            var updatedAt = entity.FindProperty(nameof(ProcessStep.UpdatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
            Assert.Equal("datetime(6)", updatedAt.GetColumnType());
        }

        [Fact]
        public void Should_Have_Index_On_ProcessId_And_StepOrder_With_Name()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                     .SequenceEqual(new[] { nameof(ProcessStep.ProcessId), nameof(ProcessStep.StepOrder) })
                );

            Assert.NotNull(idx);
            Assert.Equal("IX_ProcessStep_Process_StepOrder", idx!.GetDatabaseName());
        }

        [Fact]
        public void Should_Have_FK_To_Process_With_Cascade()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Process));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_ProcessStep_Process", fk.GetConstraintName());
        }

        [Fact]
        public void Should_Have_FK_To_Role_With_SetNull()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Role));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.SetNull, fk!.DeleteBehavior);
            Assert.Equal("FK_ProcessStep_Role", fk.GetConstraintName());
        }
    }
}