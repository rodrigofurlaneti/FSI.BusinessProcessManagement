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
            b.Property(x => x.IsActive).HasColumnName("IsActive").HasColumnType("tinyint(1)").IsRequired();

            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.HasIndex(x => x.Username).IsUnique().HasDatabaseName("UQ_Usuario_Username");
            b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_Usuario_Email");

            b.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .HasConstraintName("FK_Usuario_Department")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
