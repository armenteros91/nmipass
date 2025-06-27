using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<TenantApiKey>
{
    public void Configure(EntityTypeBuilder<TenantApiKey> builder)
    {
        builder.ToTable("TblApiKey", "Tenant");
        
        builder.HasKey(e => e.TenantApikeyId);
        
        builder.Property(e => e.TenantApikeyId)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.TenantApikeyId)
            .HasColumnName("ApiKey")
            .HasMaxLength(10)
            .IsRequired();
            
        builder.Property(e => e.TenantId)
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasMaxLength(100)
            .IsRequired(false);
            
        builder.Property(e => e.Status);
            
        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("CONVERT(DATETIME2,DATEADD(HOUR, -5,GETDATE()),120)");
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(25)
            .HasDefaultValueSql("USER_NAME()");
            
        builder.Property(e => e.LastModifiedDate)
            .IsRequired(false);
            
        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(25)
            .IsRequired(false);
            
        builder.Property(e => e.TimeStamp)
            .IsRowVersion();
            
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId);
    }
}