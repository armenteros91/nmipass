using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("PlTblTransactionsType", "Payment");

        builder.HasKey(t => t.TypeTransactionsId)
            ;
        builder.Property(t => t.Description).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Status)
            .IsRequired()
            .HasDefaultValue(true);
        // builder.Property(t => t.CreatedBy).HasMaxLength(50);
        // builder.Property(t => t.ModifiedBy).HasMaxLength(50);
        // builder.Property(t => t.TimeStamp).IsRowVersion();
    }
}
