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

        builder.HasOne(t => t.Tenant)
            .WithMany(tn => tn.Terminals) // Colección en Tenant
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índice por TenantId para acelerar búsquedas
        builder.HasIndex(tt => tt.TenantId)
            .HasDatabaseName("IDX_TblTerminalTenants_TenantId");

        // Índice único por TenantId y SecretKey
        builder.HasIndex(tt => new { tt.TenantId, tt.SecretKeyEncrypted })
            .IsUnique()
            .HasDatabaseName("UQ_Tenant_SecretKey");

        //Implementation de hash para optimizar consutlas de secretos cuando la base de datos sea muy grande 
        builder.Property(t => t.SecretKeyHash)
            .IsRequired()
            .HasMaxLength(64); // SHA-256 = 64 chars en hex

        builder.HasIndex(t => t.SecretKeyHash)
            .HasDatabaseName("IDX_Terminal_SecretHash");
    }
}