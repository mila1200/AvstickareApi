using AvstickareApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
//sätter mappen där appen körs som rot
    .SetBasePath(Directory.GetCurrentDirectory())
    //laddar in värden från appsettings.json
    .AddJsonFile("appsettings.json", optional: true)
    //laddar in miljöspecifika versionen
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    //ladda secrets
    .AddUserSecrets<Program>()
    //för att inte glömma azure
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//db connection postgre
builder.Services.AddDbContext<AvstickareContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
