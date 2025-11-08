using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> b)
        {
            b.ToTable("Department");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("DepartmentId");
            b.Property(x => x.Name).HasColumnName("DepartmentName").HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).HasColumnName("Description");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");
            b.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_Department_Name");
        }
    }
}
