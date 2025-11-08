using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class ProcessStepConfiguration : IEntityTypeConfiguration<ProcessStep>
    {
        public void Configure(EntityTypeBuilder<ProcessStep> b)
        {
            b.ToTable("ProcessStep");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("StepId");
            b.Property(x => x.ProcessId).HasColumnName("ProcessId").IsRequired();
            b.Property(x => x.StepName).HasColumnName("StepName").HasMaxLength(200).IsRequired();
            b.Property(x => x.StepOrder).HasColumnName("StepOrder").IsRequired();
            b.Property(x => x.AssignedRoleId).HasColumnName("AssignedRoleId");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");
            b.HasIndex(x => new { x.ProcessId, x.StepOrder }).HasDatabaseName("IX_ProcessStep_Process_StepOrder");
            b.HasOne<Process>()
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .HasConstraintName("FK_ProcessStep_Process")
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Role>()
                .WithMany()
                .HasForeignKey(x => x.AssignedRoleId)
                .HasConstraintName("FK_ProcessStep_Role")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
