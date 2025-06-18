using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class NmiTransactionRequestLogConfiguration : IEntityTypeConfiguration<NmiTransactionRequestLog>
{
    public void Configure(EntityTypeBuilder<NmiTransactionRequestLog> builder)
    {
        builder.ToTable("TblNmiTransactionRequestLog", "Logging");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.CreatedAt);
    }
}