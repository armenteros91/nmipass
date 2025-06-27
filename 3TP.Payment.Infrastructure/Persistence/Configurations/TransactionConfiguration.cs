using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transactions>
{
    public void Configure(EntityTypeBuilder<Transactions> builder)
    {
        builder.ToTable("TblTransactions", "Payment");

        builder.HasKey(e => e.TransactionsId);

        builder.Property(e => e.TransactionsId)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        builder.Property(t => t.paymentTransactionId).HasMaxLength(50);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.TraceId).IsRequired();

        builder.Property(e => e.TypeTransactionsId)
            .HasColumnName("TypeTransaction")
            .IsRequired();

        builder.Property(e => e.ResponseCode)
            .HasColumnName("ResponseCode")
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId);

        builder.HasOne(e => e.TransactionType)
            .WithMany(t => t.Transactions)
            .HasForeignKey(e => e.TypeTransactionsId);

        builder.HasMany(t => t.Responses)
            .WithOne(r => r.Transaction)
            .HasForeignKey(r => r.FkTransactionsId) 
            .OnDelete(DeleteBehavior.Restrict);
    }
}