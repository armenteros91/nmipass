using Microsoft.EntityFrameworkCore;
using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Entities.Nmi;
using ThreeTP.Payment.Domain.Entities.Payments;
using ThreeTP.Payment.Domain.Entities.Tenant; // Tenant is used
// TenantApiKey is no longer used directly here for DbSet

using ThreeTP.Payment.Infrastructure.Persistence.Configurations;

namespace ThreeTP.Payment.Infrastructure.Persistence;

public class NmiDbContext : DbContext
{
    public DbSet<NmiTransactionRequestLog> NmiRequestLogs { get; set; }
    public DbSet<NmiTransactionResponseLog> NmiResponseLogs { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    // public DbSet<TenantApiKey> ApiKeys { get; set; } // Removed
    public DbSet<Terminal> Terminals { get; set; }
    public DbSet<Transactions> Transactions { get; set; }
    public DbSet<TransactionResponse> TransactionResponse { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }

    public NmiDbContext(DbContextOptions<NmiDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NMI (Logging schema)
        modelBuilder.Entity<NmiTransactionRequestLog>().ToTable("NmiRequestLogs", "Logging");
        modelBuilder.Entity<NmiTransactionResponseLog>().ToTable("NmiResponseLogs", "Logging");

        // Tenant schema
        modelBuilder.Entity<Tenant>().ToTable(nameof(Tenant), "Tenant");
        // modelBuilder.Entity<TenantApiKey>().ToTable("ApiKeys", "Tenant"); // Removed
        modelBuilder.Entity<Terminal>().ToTable(nameof(Terminal), "Tenant");

        // Payment schema
        modelBuilder.Entity<Transactions>().ToTable(nameof(Transactions), "Payment");
        modelBuilder.Entity<TransactionResponse>().ToTable(nameof(TransactionResponse), "Payment");
        modelBuilder.Entity<TransactionType>().ToTable(nameof(TransactionType), "Payment");


        // Aplica configuración base a todas las entidades que heredan de BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(BaseEntity.CreatedDate))
                    .IsRequired()
                    .HasDefaultValueSql("CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)");

                builder.Property(nameof(BaseEntity.CreatedBy))
                    .HasMaxLength(25)
                    .HasDefaultValueSql("USER_NAME()");

                builder.Property(nameof(BaseEntity.ModifiedBy))
                    .HasMaxLength(25);

                builder.Property(nameof(BaseEntity.TimeStamp))
                    .IsRowVersion();
            }
        }

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new TerminalConfiguration());
        // modelBuilder.ApplyConfiguration(new TenantApiKeyConfiguration()); // Removed

        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionResponseConfiguration());

        modelBuilder.ApplyConfiguration(new NmiTransactionRequestLogConfiguration());
        modelBuilder.ApplyConfiguration(new NmiTransactionResponseLogConfiguration());
    }
}