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

            // Mapeie para o nome REAL da coluna no banco:
            b.Property(x => x.Name)
             .HasColumnName("RoleName") // <<< troque para o nome que existe
             .HasMaxLength(100)
             .IsRequired();

            b.Property(x => x.Description).HasColumnName("Description");
            b.Property(x => x.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime(6)");
        }
    }
}
