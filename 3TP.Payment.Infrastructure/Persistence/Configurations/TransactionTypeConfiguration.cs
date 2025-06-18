using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("PlTblTransactionsType", "Payment");

        builder.HasKey(t => t.TypeTransactionsId);
        builder.Property(t => t.Description).HasMaxLength(50).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(25);
        builder.Property(t => t.ModifiedBy).HasMaxLength(25);
        builder.Property(t => t.TimeStamp).IsRowVersion();
    }
}
