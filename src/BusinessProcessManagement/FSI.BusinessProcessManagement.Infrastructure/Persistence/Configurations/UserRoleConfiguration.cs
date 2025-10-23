using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> b)
        {
            b.ToTable("UserRole");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("UserRoleId");

            b.Property(x => x.UserId).HasColumnName("UserId").IsRequired();
            b.Property(x => x.RoleId).HasColumnName("RoleId").IsRequired();
            b.Property(x => x.AssignedAt).HasColumnName("AssignedAt").HasColumnType("datetime(6)");

            b.Property(x => x.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime(6)");

            b.HasIndex(x => new { x.UserId, x.RoleId })
             .IsUnique()
             .HasDatabaseName("UQ_UserRole_User_Role");

            b.HasOne(ur => ur.User)
             .WithMany(u => u.UserRoles)
             .HasForeignKey(ur => ur.UserId)
             .HasConstraintName("FK_UserRole_User")
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ur => ur.Role)
             .WithMany(r => r.UserRoles)
             .HasForeignKey(ur => ur.RoleId)
             .HasConstraintName("FK_UserRole_Role")
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
