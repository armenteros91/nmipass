using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class NmiTransactionResponseLogConfiguration : IEntityTypeConfiguration<NmiTransactionResponseLog>
{
    public void Configure(EntityTypeBuilder<NmiTransactionResponseLog> builder)
    {
        builder.ToTable("TblNmiTransactionResponseLog", "Logging");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.TransactionId);
        builder.Property(x => x.RawResponse).IsRequired();
        builder.Property(x => x.ReceivedAt);

        builder.HasOne(x => x.Request)
            .WithMany()
            .HasForeignKey(x => x.RequestId);
    }
}