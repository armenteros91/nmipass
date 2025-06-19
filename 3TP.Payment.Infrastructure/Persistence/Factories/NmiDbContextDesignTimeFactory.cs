// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
// using Microsoft.Extensions.Configuration;
//
// namespace ThreeTP.Payment.Infrastructure.Persistence.Factories;
//
// /// <summary>
// /// A factory for creating <see cref="NmiDbContext"/> instances at design time, used by BaseEntity Framework Core tools
// /// (e.g., migrations).
// /// </summary>
// public class NmiDbContextDesignTimeFactory : IDesignTimeDbContextFactory<NmiDbContext>
// {
//     /// <summary>
//     /// Creates a new instance of <see cref="NmiDbContext"/> with the specified configuration.
//     /// </summary>
//     /// <param name="args">Command-line arguments passed to the EF Core CLI (not used in this implementation).</param>
//     /// <returns>A configured instance of <see cref="NmiDbContext"/>.</returns>
//     /// <exception cref="InvalidOperationException">Thrown if the 'NmiDb' connection string is missing.</exception>
//     public NmiDbContext CreateDbContext(string[] args)
//     {
//         // Step 1: Load configuration from appsettings.json and environment variables
//         var basePath = Directory.GetCurrentDirectory();
//         Console.WriteLine($"Base path: {basePath}");
//         var config = new ConfigurationBuilder()
//             .SetBasePath(basePath)
//             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//             .AddEnvironmentVariables()
//             .Build();
//
//         // Step 2: Retrieve the connection string
//         var connectionString = config.GetConnectionString("NmiDb");
//         Console.WriteLine($"Connection string: {connectionString}");
//         if (string.IsNullOrEmpty(connectionString))
//         {
//             throw new InvalidOperationException("The 'NmiDb' connection string is missing or not configured.");
//         }
//
//         // Step 3: Configure the DbContext options
//         var optionsBuilder = new DbContextOptionsBuilder<NmiDbContext>();
//         optionsBuilder.UseSqlServer(connectionString, options =>
//         {
//             options.EnableRetryOnFailure(); // Optional: Adds resilience to transient failures
//         });
//
//         // Step 4: Create and return the DbContext instance
//         return new NmiDbContext(optionsBuilder.Options);
//     }
// }