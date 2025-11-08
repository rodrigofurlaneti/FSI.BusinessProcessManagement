using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Configurations
{
    public class ScreenConfiguration : IEntityTypeConfiguration<Screen>
    {
        public void Configure(EntityTypeBuilder<Screen> b)
        {
            b.ToTable("Screen");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("ScreenId");
            b.Property(x => x.Name).HasColumnName("ScreenName").HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).HasColumnName("Description");
            b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
            b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)");
            b.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_Screen_Name");
        }
    }
}
