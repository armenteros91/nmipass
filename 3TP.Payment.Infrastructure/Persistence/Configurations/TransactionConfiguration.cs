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
            
        builder.Property(e => e.TenantId)
            .IsRequired();
            
        builder.Property(e => e.TraceId)
            .IsRequired();
            
        builder.Property(e => e.TypeTransactionsId)
            .HasColumnName("TypeTransaction")
            .IsRequired();

        // builder.Property(e => e.Monto)
        //    .HasColumnName("Monto")
        //    .IsRequired();
        //
        // builder.Property(e => e.orderId)
        //   .HasColumnName("orderId")
        //   .IsRequired();
        //
        // builder.Property(e => e.AuthCode)
        //   .HasColumnName("AuthCode")
        //   .IsRequired();

        builder.Property(e => e.response_code)
         .HasColumnName("ResponseCode")
         .IsRequired()
         .HasConversion<int>();

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("CONVERT(DATETIME2,DATEADD(HOUR, -5,GETDATE()),120)");
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(25)
            .HasDefaultValueSql("USER_NAME()");
            
        builder.Property(e => e.ModifiedDate)
            .IsRequired(false);
            
        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(25)
            .IsRequired(false);
            
        builder.Property(e => e.TimeStamp)
            .IsRowVersion();
            
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId);
            
        builder.HasOne(e => e.TransactionType)
            .WithMany(t => t.Transactions)
            .HasForeignKey(e => e.TypeTransactionsId);

    }
}