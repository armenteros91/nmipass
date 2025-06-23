using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> builder)
    {
        builder.ToTable("TblTerminalTenants", "Tenant");

        builder.HasKey(tt => tt.TerminalId);

        builder.Property(t => t.SecretKeyEncrypted)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(tt => tt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // builder.Property(tt => tt.CreatedDate)
        //     .IsRequired()
        //     .HasDefaultValueSql("CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)");
        //
        // builder.Property(tt => tt.CreatedBy)
        //     .HasMaxLength(50)
        //     .HasDefaultValueSql("USER_NAME()");
        //
        // builder.Property(tt => tt.TimeStamp)
        //     .IsRowVersion();

        // This side of the relationship is configured in TenantConfiguration.
        // builder.HasOne(t => t.Tenant)
        //    .WithOne(tn => tn.Terminal) // Corrected from WithMany
        //    .HasForeignKey<Terminal>(t => t.TenantId) // Type specified for clarity
        //    .OnDelete(DeleteBehavior.Cascade);
        
        // Índice por TenantId para acelerar búsquedas.
        // For a 1-to-1 relationship, TenantId should be unique.
        builder.HasIndex(tt => tt.TenantId)
            .IsUnique() // TenantId must be unique as each Tenant has only one Terminal
            .HasDatabaseName("UQ_TblTerminalTenants_TenantId");

        // Índice único por SecretKeyEncrypted (assuming secret keys should be globally unique or unique per tenant)
        // If SecretKeyEncrypted should be globally unique:
        builder.HasIndex(tt => tt.SecretKeyEncrypted)
            .IsUnique()
            .HasDatabaseName("UQ_Terminal_SecretKeyEncrypted");
        // If SecretKeyEncrypted only needs to be unique per tenant, the UQ_TblTerminalTenants_TenantId ensures this.
        // The previous index UQ_Tenant_SecretKey (TenantId, SecretKeyEncrypted) is no longer needed
        // because TenantId itself is now unique.

        //Implementation de hash para optimizar consutlas de secretos cuando la base de datos sea muy grande 
        builder.Property(t => t.SecretKeyHash)
            .IsRequired()
            .HasMaxLength(64); // SHA-256 = 64 chars en hex

        builder.HasIndex(t => t.SecretKeyHash)
            .HasDatabaseName("IDX_Terminal_SecretHash");
    }
}