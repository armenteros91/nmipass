using Microsoft.EntityFrameworkCore;
using ThreeTP.Payment.Domain.Entities.Nmi;
using ThreeTP.Payment.Domain.Entities.Payments;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Infrastructure.Persistence.Configurations;

namespace ThreeTP.Payment.Infrastructure.Persistence;

public class NmiDbContext : DbContext
{
    public DbSet<NmiTransactionRequestLog> NmiRequestLogs { get; set; }
    public DbSet<NmiTransactionResponseLog> NmiResponseLogs { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantApiKey> ApiKeys { get; set; }
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
        modelBuilder.Entity<NmiTransactionRequestLog>().ToTable("NmiRequestLogs", "Payment");
        modelBuilder.Entity<NmiTransactionResponseLog>().ToTable("NmiResponseLogs", "Payment");
        modelBuilder.Entity<Tenant>().ToTable(nameof(Tenant), "Tenant");
        modelBuilder.Entity<TenantApiKey>().ToTable("ApiKeys", "Tenant");

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new ApiKeyConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionTypeConfiguration());
    }
}