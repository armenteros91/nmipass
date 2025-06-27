using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TenantApiKeyConfiguration : IEntityTypeConfiguration<TenantApiKey>
{
    public void Configure(EntityTypeBuilder<TenantApiKey> builder)
    {
        builder.ToTable("TblApiKey", "Tenant");
        builder.HasKey(x => x.TenantApikeyId);

        builder.Property(x => x.ApiKeyValue)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(true);

        // Relación explícita: Un TenantApiKey pertenece a un Tenant,
        // y un Tenant tiene una propiedad de navegación ApiKey que apunta a este TenantApiKey.
        builder.HasOne(apiKey => apiKey.Tenant)
            .WithOne(tenant => tenant.ApiKey)
            .HasForeignKey<TenantApiKey>(apiKey => apiKey.TenantId)
            .OnDelete(DeleteBehavior.Restrict);


    }
}