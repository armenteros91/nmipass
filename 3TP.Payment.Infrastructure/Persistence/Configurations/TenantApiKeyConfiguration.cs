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

        // Relación explícita
        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.ApiKeys) // Asegúrate de tener esta colección en Tenant
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);


        //     builder.ToTable("TenantApiKeys", "Tenant");
        //
        //     builder.HasKey(e => e.TenantApikeyId);
        //
        //     builder.Property(e => e.TenantApikeyId)
        //         .IsRequired()
        //         .HasDefaultValueSql("NEWID()");
        //
        //     builder.Property(e => e.TenantApikeyId)
        //         .HasColumnName("ApiKey")
        //         .HasMaxLength(10)
        //         .IsRequired();
        //
        //     builder.Property(e => e.TenantId)
        //         .IsRequired();
        //
        //     builder.Property(e => e.Description)
        //         .HasMaxLength(100)
        //         .IsRequired(false);
        //
        //     builder.Property(e => e.Status);
        //
        //     builder.Property(e => e.CreatedDate)
        //         .IsRequired()
        //         .HasDefaultValueSql("CONVERT(DATETIME2,DATEADD(HOUR, -5,GETDATE()),120)");
        //

    }
}