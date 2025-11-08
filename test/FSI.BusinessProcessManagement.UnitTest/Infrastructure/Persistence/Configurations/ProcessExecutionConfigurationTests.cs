using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;
using System.Linq;

namespace FSI.BusinessProcessManagement.UnitTests.Infrastructure.Persistence.Configurations
{
    public class ProcessExecutionConfigurationTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            public DbSet<ProcessExecution> ProcessExecutions => Set<ProcessExecution>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new ProcessExecutionConfiguration());
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
            var entity = model.FindEntityType(typeof(ProcessExecution))!;
            return (model, entity);
        }

        [Fact]
        public void Should_Map_To_Correct_Table_And_PrimaryKey()
        {
            var (_, entity) = BuildModel();
            Assert.Equal("ProcessExecution", entity.GetTableName());

            var pk = entity.FindPrimaryKey();
            Assert.NotNull(pk);
            Assert.Single(pk!.Properties);

            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal("ExecutionId", pk.Properties[0].GetColumnName(store));
        }

        [Fact]
        public void Should_Map_Columns_Correctly()
        {
            var (_, entity) = BuildModel();
            var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

            var processId = entity.FindProperty(nameof(ProcessExecution.ProcessId))!;
            Assert.Equal("ProcessId", processId.GetColumnName(store));
            Assert.False(processId.IsNullable);

            var stepId = entity.FindProperty(nameof(ProcessExecution.StepId))!;
            Assert.Equal("StepId", stepId.GetColumnName(store));
            Assert.False(stepId.IsNullable);

            var userId = entity.FindProperty(nameof(ProcessExecution.UserId))!;
            Assert.Equal("UserId", userId.GetColumnName(store));

            var status = entity.FindProperty(nameof(ProcessExecution.Status))!;
            Assert.Equal("Status", status.GetColumnName(store));
            Assert.False(status.IsNullable);
            Assert.Equal(50, status.GetMaxLength());

            var started = entity.FindProperty(nameof(ProcessExecution.StartedAt))!;
            Assert.Equal("datetime(6)", started.GetColumnType());

            var completed = entity.FindProperty(nameof(ProcessExecution.CompletedAt))!;
            Assert.Equal("datetime(6)", completed.GetColumnType());

            var remarks = entity.FindProperty(nameof(ProcessExecution.Remarks))!;
            Assert.Equal("Remarks", remarks.GetColumnName(store));

            var createdAt = entity.FindProperty(nameof(ProcessExecution.CreatedAt))!;
            Assert.Equal("datetime(6)", createdAt.GetColumnType());
        }

        [Fact]
        public void Should_Have_Index_On_ProcessId_And_Status()
        {
            var (_, entity) = BuildModel();

            var idx = entity.GetIndexes()
                .FirstOrDefault(i =>
                    i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { nameof(ProcessExecution.ProcessId), nameof(ProcessExecution.Status) })
                );

            Assert.NotNull(idx);
            Assert.Equal("IX_Execution_Process_Status", idx!.GetDatabaseName());
        }

        [Fact]
        public void Should_Have_FK_To_Process_With_Cascade()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Process));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_ProcessExecution_Process", fk.GetConstraintName());
        }

        [Fact]
        public void Should_Have_FK_To_ProcessStep_With_Cascade()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(ProcessStep));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
            Assert.Equal("FK_ProcessExecution_Step", fk.GetConstraintName());
        }

        [Fact]
        public void Should_Have_FK_To_User_With_SetNull()
        {
            var (_, entity) = BuildModel();

            var fk = entity.GetForeignKeys()
                .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(User));

            Assert.NotNull(fk);
            Assert.Equal(DeleteBehavior.SetNull, fk!.DeleteBehavior);
            Assert.Equal("FK_ProcessExecution_User", fk.GetConstraintName());
        }
    }
}