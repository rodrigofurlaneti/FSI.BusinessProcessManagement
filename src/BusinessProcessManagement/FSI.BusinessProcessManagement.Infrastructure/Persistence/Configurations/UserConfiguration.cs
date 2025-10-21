using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Usuario");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("UserId");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.DepartmentId).HasColumnName("DepartmentId");

            b.Property(x => x.Username).IsRequired().HasMaxLength(100).HasColumnName("Username");
            b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255).HasColumnName("PasswordHash");
            b.Property(x => x.Email).HasMaxLength(200).HasColumnName("Email");
            b.Property(x => x.IsActive).HasColumnName("IsActive");

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
