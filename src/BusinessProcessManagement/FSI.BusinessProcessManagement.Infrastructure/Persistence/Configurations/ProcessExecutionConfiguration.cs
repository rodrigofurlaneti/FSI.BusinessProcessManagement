using FSI.BusinessProcessManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class ProcessExecutionConfiguration : IEntityTypeConfiguration<ProcessExecution>
    {
        public void Configure(EntityTypeBuilder<ProcessExecution> b)
        {
            b.ToTable("ProcessExecution");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("ExecutionId");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.Property(x => x.ProcessId).HasColumnName("ProcessId");
            b.Property(x => x.StepId).HasColumnName("StepId");
            b.Property(x => x.UserId).HasColumnName("UserId");
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).HasColumnName("Status");
            b.Property(x => x.StartedAt).HasColumnType("datetime(6)");
            b.Property(x => x.CompletedAt).HasColumnType("datetime(6)");
            b.Property(x => x.Remarks).HasColumnType("text");

            b.HasIndex(x => new { x.ProcessId, x.Status }).HasDatabaseName("IX_Execution_Process_Status");

            b.HasOne<Process>().WithMany().HasForeignKey(x => x.ProcessId)
                .HasConstraintName("FK_ProcessExecution_Process")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<ProcessStep>().WithMany().HasForeignKey(x => x.StepId)
                .HasConstraintName("FK_ProcessExecution_Step")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId)
                .HasConstraintName("FK_ProcessExecution_User")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
