using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetcoreHRIS.Entities;

namespace NetcoreHRIS.Data.Configurations;

public class LeaveAllowanceConfiguration : IEntityTypeConfiguration<LeaveAllowance>
{
    public void Configure(EntityTypeBuilder<LeaveAllowance> builder)
    {
        builder.ToTable("leave_allowances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(x => x.LeaveMasterId).HasColumnName("leave_master_id").IsRequired();
        builder.Property(x => x.Year).HasColumnName("year").IsRequired();
        builder.Property(x => x.QuotaDays).HasColumnName("quota_days").IsRequired();
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.EmployeeId, x.LeaveMasterId, x.Year })
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveMaster)
            .WithMany()
            .HasForeignKey(x => x.LeaveMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
