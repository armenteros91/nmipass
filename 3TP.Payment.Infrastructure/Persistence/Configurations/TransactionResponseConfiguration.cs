using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TransactionResponseConfiguration : IEntityTypeConfiguration<TransactionResponse>
{
    public void Configure(EntityTypeBuilder<TransactionResponse> builder)
    {
        builder.ToTable("TblTransactionResponse", "Payment");

        builder.HasKey(t => t.TransaccionResponseId);

        builder.Property(t => t.Response).HasMaxLength(1);
        builder.Property(t => t.ResponseText).HasMaxLength(250);
        builder.Property(t => t.AuthCode).HasMaxLength(20);
        builder.Property(t => t.TransactionId).HasMaxLength(100);
        builder.Property(t => t.AvsResponse).HasMaxLength(10);
        builder.Property(t => t.CvvResponse).HasMaxLength(10);
        builder.Property(t => t.OrderId).HasMaxLength(50);
        builder.Property(t => t.ResponseCode).HasMaxLength(10);
        builder.Property(t => t.EmvAuthResponseData).HasMaxLength(255);
        builder.Property(t => t.CustomerVaultId).HasMaxLength(100);
        builder.Property(t => t.KountScore).HasMaxLength(10);
        builder.Property(t => t.MerchantAdviceCode).HasMaxLength(10);

        builder.HasOne(r => r.Transaction)
            .WithMany(t => t.Responses)
            .HasForeignKey(r => r.FkTransactionsId);
    }
}