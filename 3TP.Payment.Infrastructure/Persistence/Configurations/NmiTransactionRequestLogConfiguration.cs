using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class NmiTransactionRequestLogConfiguration : IEntityTypeConfiguration<NmiTransactionRequestLog>
{
    public void Configure(EntityTypeBuilder<NmiTransactionRequestLog> builder)
    {
        builder.ToTable("TblNmiTransactionRequestLog", "Logging");

        builder.HasKey(x => x.NmiTransactionRequestLogId);
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PayloadJson)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(x => x.OrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RawContent)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");
        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderId");
        builder.HasIndex(x => x.CreatedDate).HasDatabaseName("IX_CreatedAt");
        
    }
}