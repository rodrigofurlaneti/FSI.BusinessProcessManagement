using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class RoleScreenPermissionConfiguration : IEntityTypeConfiguration<RoleScreenPermission>
    {
        public void Configure(EntityTypeBuilder<RoleScreenPermission> b)
        {
            b.ToTable("RoleScreenPermission");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("RoleScreenPermissionId");
            b.Property(x => x.RoleId).HasColumnName("RoleId").IsRequired();
            b.Property(x => x.ScreenId).HasColumnName("ScreenId").IsRequired();
            b.Property(x => x.CanView).HasColumnName("CanView").HasColumnType("tinyint(1)").IsRequired();
            b.Property(x => x.CanCreate).HasColumnName("CanCreate").HasColumnType("tinyint(1)").IsRequired();
            b.Property(x => x.CanEdit).HasColumnName("CanEdit").HasColumnType("tinyint(1)").IsRequired();
            b.Property(x => x.CanDelete).HasColumnName("CanDelete").HasColumnType("tinyint(1)").IsRequired();
            b.HasIndex(x => new { x.RoleId, x.ScreenId }).IsUnique();
            b.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId)
                .HasConstraintName("FK_RoleScreenPermission_Role")
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Screen>().WithMany().HasForeignKey(x => x.ScreenId)
                .HasConstraintName("FK_RoleScreenPermission_Screen")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
