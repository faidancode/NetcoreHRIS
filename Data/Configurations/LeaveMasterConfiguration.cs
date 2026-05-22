using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetcoreHRIS.Entities;

namespace NetcoreHRIS.Data.Configurations;

public class LeaveMasterConfiguration : IEntityTypeConfiguration<LeaveMaster>
{
    public void Configure(EntityTypeBuilder<LeaveMaster> builder)
    {
        builder.ToTable("leave_masters");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.QuotaDays).HasColumnName("quota_days").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.Name).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => x.Code).IsUnique().HasFilter("is_deleted = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
