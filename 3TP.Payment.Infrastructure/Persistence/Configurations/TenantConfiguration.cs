using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("TblTenant", "Tenant");

        builder.HasKey(e => e.TenantId);

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CompanyCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // builder.Property(e => e.CreatedDate)
        //     .IsRequired()
        //     .HasDefaultValueSql("CONVERT(DATETIME2,DATEADD(HOUR, -5,GETDATE()),120)");
        //
        // builder.Property(e => e.CreatedBy)
        //     .HasMaxLength(50)
        //     .HasDefaultValueSql("USER_NAME()");
        //
        // builder.Property(e => e.ModifiedDate)
        //     .IsRequired(false);
        //
        // builder.Property(e => e.ModifiedBy)
        //     .HasMaxLength(50)
        //     .IsRequired(false)
        //     .HasDefaultValueSql("USER_NAME()");
        //
        // builder.Property(e => e.TimeStamp)
        //     .IsRowVersion();

        // Relación 1:N
        builder.HasMany(t => t.Terminals)
            .WithOne(tt => tt.Tenant)
            .HasForeignKey(tt => tt.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}