using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Persistence.Configurations;

public class NmiTransactionResponseLogConfiguration : IEntityTypeConfiguration<NmiTransactionResponseLog>
{
    public void Configure(EntityTypeBuilder<NmiTransactionResponseLog> builder)
    {
        builder.ToTable("TblNmiTransactionResponseLog", "Logging");

        builder.HasKey(x => x.NmiTransactionResponseLogId);
        
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.TransactionId)
            .HasMaxLength(100);

        builder.Property(x => x.RawResponse)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.HasIndex(x => x.RequestId).HasDatabaseName("IX_TblNmiTransactionResponseLog_RequestId");
        builder.HasIndex(x => x.TransactionId).HasDatabaseName("IX_TblNmiTransactionResponseLog_TransactionId");
        builder.HasIndex(x => x.CreatedDate).HasDatabaseName("IX_TblNmiTransactionResponseLog_CreatedDate");

        builder.HasOne(x => x.Request)
            .WithMany(x=>x.Responses)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_TblNmiTransactionResponseLog_TblNmiTransactionRequestLog_RequestId");
    }
}