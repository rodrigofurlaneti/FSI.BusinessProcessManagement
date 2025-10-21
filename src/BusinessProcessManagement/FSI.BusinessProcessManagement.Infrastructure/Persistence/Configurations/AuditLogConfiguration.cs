using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> b)
        {
            b.ToTable("AuditLog");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("AuditId");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");

            b.Property(x => x.UserId).HasColumnName("UserId");
            b.Property(x => x.ScreenId).HasColumnName("ScreenId");
            b.Property(x => x.ActionType).IsRequired().HasMaxLength(60);
            b.Property(x => x.ActionTimestamp).HasColumnType("datetime(6)");
            b.Property(x => x.AdditionalInfo).HasColumnName("AdditionalInfo");

            b.HasIndex(x => new { x.UserId, x.ActionTimestamp }).HasDatabaseName("IX_Audit_User_Time");

            b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId)
                .HasConstraintName("FK_AuditLog_User")
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne<Screen>().WithMany().HasForeignKey(x => x.ScreenId)
                .HasConstraintName("FK_AuditLog_Screen")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
