using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("User");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("UserId");
            b.Property(x => x.DepartmentId).HasColumnName("DepartmentId");
            b.Property(x => x.Username).HasColumnName("Username").HasMaxLength(100).IsRequired();
            b.Property(x => x.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255).IsRequired();
            b.Property(x => x.Email).HasColumnName("Email").HasMaxLength(200);
            b.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime(6)");
            b.HasMany(u => u.UserRoles)
             .WithOne(ur => ur.User)
             .HasForeignKey(ur => ur.UserId)
             .HasConstraintName("FK_UserRole_User")
             .OnDelete(DeleteBehavior.Cascade);
            b.Metadata.FindNavigation(nameof(User.UserRoles))!
                      .SetPropertyAccessMode(PropertyAccessMode.Field); 
        }
    }
}
