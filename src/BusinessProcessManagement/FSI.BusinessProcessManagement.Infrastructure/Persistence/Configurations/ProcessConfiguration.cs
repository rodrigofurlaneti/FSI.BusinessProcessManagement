using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class ProcessConfiguration : IEntityTypeConfiguration<DomainProcess>
    {
        public void Configure(EntityTypeBuilder<DomainProcess> b)
        {
            b.ToTable("ProcessoBPM");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("ProcessId");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.Property(x => x.DepartmentId).HasColumnName("DepartmentId");
            b.Property(x => x.Name).IsRequired().HasMaxLength(200).HasColumnName("ProcessName");
            b.Property(x => x.Description).HasColumnType("text");
            b.Property(x => x.CreatedById).HasColumnName("CreatedBy");

            b.HasIndex(x => new { x.DepartmentId, x.Name }).IsUnique().HasDatabaseName("UQ_ProcessName_Department");

            b.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId)
                .HasConstraintName("FK_Process_Department")
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedById)
                .HasConstraintName("FK_Process_CreatedBy")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
