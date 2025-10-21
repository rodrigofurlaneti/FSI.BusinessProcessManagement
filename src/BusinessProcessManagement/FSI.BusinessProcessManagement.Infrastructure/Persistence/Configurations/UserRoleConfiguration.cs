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
            b.Property(x => x.AssignedAt).HasColumnType("datetime(6)");

            b.Property(x => x.UserId).HasColumnName("UserId");
            b.Property(x => x.RoleId).HasColumnName("RoleId");

            b.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique()
                .HasDatabaseName("UQ_UserRole_User_Role");

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .HasConstraintName("FK_UserRole_User")
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Role>()
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .HasConstraintName("FK_UserRole_Role")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
