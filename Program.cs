using System.Text;
using AvstickareApi.Data;
using AvstickareApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


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

//hämta jwt-secrets från user secrets
var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT key saknas");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("Issuer saknas");
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("Audience saknas");

//registrera authservice som reggar jwt
builder.Services.AddTransient<AuthService>();

//autentisering. Varje inkommande anrop kommer autetiseras med JWT-bearer. Om inte ok, 401.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        //validera användare och audience
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        //se till att den inte gått ut
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        //omvandla och verifiera signatur
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

//Cors-inställningar för utveckling
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontend",
    policy =>
    {
        policy.WithOrigins("http://localhost:5247")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

//ta in services
builder.Services.AddHttpClient<PlaceService>();
builder.Services.AddHttpClient<RouteService>();
builder.Services.AddHttpClient<SuggestedPlaceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //i utvecklingsläge, mer detlajerade felmeddelanden
    app.UseDeveloperExceptionPage();
}
else
{
    //i produktion, omdirigera till endpoint error
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();



app.Run();
