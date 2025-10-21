using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> b)
        {
            b.ToTable("Role");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("RoleId");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.Property(x => x.Name).IsRequired().HasMaxLength(100).HasColumnName("RoleName");
            b.Property(x => x.Description).HasColumnType("text");

            b.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_Role_Name");
        }
    }
}
