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
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.Property(x => x.RoleId).HasColumnName("RoleId");
            b.Property(x => x.ScreenId).HasColumnName("ScreenId");
            b.Property(x => x.CanView).HasColumnName("CanView");
            b.Property(x => x.CanCreate).HasColumnName("CanCreate");
            b.Property(x => x.CanEdit).HasColumnName("CanEdit");
            b.Property(x => x.CanDelete).HasColumnName("CanDelete");

            b.HasIndex(x => new { x.RoleId, x.ScreenId }).IsUnique().HasDatabaseName("UQ_Role_Screen");

            b.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId)
                .HasConstraintName("FK_RSP_Role")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Screen>().WithMany().HasForeignKey(x => x.ScreenId)
                .HasConstraintName("FK_RSP_Screen")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
