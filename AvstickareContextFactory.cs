using AvstickareApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AvstickareApi;
//för att kunna uppdatera databasen när http nu används
public class AvstickareContextFactory : IDesignTimeDbContextFactory<AvstickareContext>
{
    public AvstickareContext CreateDbContext(string[] args)
    {
        //ladda konfiguration från appsettings + user secrets
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<AvstickareContextFactory>()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AvstickareContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AvstickareContext(optionsBuilder.Options);
    }
}