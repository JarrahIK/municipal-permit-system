using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MunicipalPermitSystem.Api.Configuration;
using MunicipalPermitSystem.Api.Data;
using MunicipalPermitSystem.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Settings (+ fail fast on missing / insecure secrets) ----------
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var securitySettings = builder.Configuration.GetSection("Security").Get<SecuritySettings>() ?? new SecuritySettings();
StartupValidation.Validate(builder.Environment, jwtSettings, securitySettings);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));

// ---------- Database ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IFieldEncryptor, AesGcmFieldEncryptor>();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// ---------- Auth ----------
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IAccountStore, EfAccountStore>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep claim names exactly as written in the token ("sub", "name", "role").
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---------- CORS (allowed React origins come from configuration) ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Startup tasks: migrations + demo data ----------
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        if (!db.Database.GetMigrations().Any())
            throw new InvalidOperationException(
                "No database migrations found. Create the first one once: in Visual Studio open " +
                "Tools > NuGet Package Manager > Package Manager Console and run:  Add-Migration InitialCreate");

        await db.Database.MigrateAsync();
    }

    if (app.Configuration.GetValue<bool>("Seed:DemoData"))
    {
        if (app.Environment.IsProduction())
            throw new InvalidOperationException("Refusing to seed demo accounts in Production.");

        await DevDataSeeder.SeedAsync(db);
    }
}

// ---------- HTTP pipeline ----------
if (app.Environment.IsDevelopment())
{
    // API docs only outside production.
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Configuration.GetValue<bool>("Https:Redirect"))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", async (ApplicationDbContext db, IHostEnvironment env, CancellationToken ct) =>
        await db.Database.CanConnectAsync(ct)
            ? Results.Ok(new { status = "ok", env = env.EnvironmentName })
            : Results.Problem("Database unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable))
    .WithName("Health");

app.Run();

// Lets the test project start the whole app later (WebApplicationFactory).
public partial class Program;
