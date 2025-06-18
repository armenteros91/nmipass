using Microsoft.EntityFrameworkCore;

namespace ThreeTP.Payment.Infrastructure.Persistence.Factories;

public interface INmiDbContextFactory
{
    NmiDbContext CreateDbContext();
}

public class NmiDbContextFactory : INmiDbContextFactory
{
    private readonly IDbContextFactory<NmiDbContext> _factory;

    public NmiDbContextFactory(IDbContextFactory<NmiDbContext> factory)
    {
        _factory = factory;
    }

    public NmiDbContext CreateDbContext()
    {
        return _factory.CreateDbContext();
    }
}