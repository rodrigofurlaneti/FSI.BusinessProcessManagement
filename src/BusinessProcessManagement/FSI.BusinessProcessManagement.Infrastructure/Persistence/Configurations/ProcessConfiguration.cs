using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class ProcessConfiguration : IEntityTypeConfiguration<Process>
    {
        public void Configure(EntityTypeBuilder<Process> b)
        {
            b.ToTable("ProcessoBPM");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("ProcessId");

            b.Property(x => x.Name).HasColumnName("ProcessName").HasMaxLength(200).IsRequired();
            b.Property(x => x.DepartmentId).HasColumnName("DepartmentId");
            b.Property(x => x.Description).HasColumnName("Description");
            b.Property(x => x.CreatedBy).HasColumnName("CreatedBy");

            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.HasIndex(x => new { x.DepartmentId, x.Name }).HasDatabaseName("UQ_ProcessName_Department");
        }
    }
}
