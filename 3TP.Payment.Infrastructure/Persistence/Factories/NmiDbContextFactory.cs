using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace ThreeTP.Payment.Infrastructure.Persistence.Factories;


public interface INmiDbContextFactory
{
    NmiDbContext CreateDbContext();
}

public class NmiDbContextFactory : INmiDbContextFactory, IDesignTimeDbContextFactory<NmiDbContext>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NmiDbContextFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public NmiDbContext CreateDbContext()
    {
        var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<NmiDbContext>>()
            .CreateDbContext();

        // Asociar el contexto con el scope para limpiar correctamente
        return new ScopedDbContextWrapper(context, scope);
    }

    public NmiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NmiDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=TuBase;Trusted_Connection=True;");

        return new NmiDbContext(optionsBuilder.Options);
    }

    private class ScopedDbContextWrapper : NmiDbContext
    {
        private readonly IServiceScope _scope;
        private readonly NmiDbContext _inner;

        public ScopedDbContextWrapper(NmiDbContext inner, IServiceScope scope)
            : base(new DbContextOptions<NmiDbContext>())
        {
            _inner = inner;
            _scope = scope;
        }

        public override void Dispose()
        {
            _inner.Dispose();
            _scope.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            _scope.Dispose();
        }
    }
}