using System.Net;
using System.Net.Sockets;
using System.Text;
using Jabartah.Trivia.Api.Endpoints;
using Jabartah.Trivia.Api.Security;
using Jabartah.Trivia.Application;
using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Infrastructure;
using Jabartah.Trivia.Infrastructure.Persistence;
using Jabartah.Trivia.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var allowedFrontendPort = builder.Configuration["Cors:AllowedFrontendPort"] ?? "3030";

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin => IsAllowedOrigin(origin, allowedFrontendPort))
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// The clue-giver's phone (Password mode's QR flow) needs to reach this API from a different
// device on the same network -- a single fixed origin (e.g. "http://localhost:3030") breaks
// the moment the shared screen is loaded via a LAN IP instead. This allows any private-network
// host (RFC1918) or localhost on the configured frontend port, so it works on any home network
// without per-network config edits.
static bool IsAllowedOrigin(string origin, string allowedPort)
{
    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
    if (uri.Port.ToString() != allowedPort) return false;

    var host = uri.Host;
    if (host is "localhost" or "127.0.0.1") return true;
    if (!IPAddress.TryParse(host, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork) return false;

    var b = ip.GetAddressBytes();
    return b[0] == 10                                  // 10.0.0.0/8
        || (b[0] == 172 && b[1] is >= 16 and <= 31)     // 172.16.0.0/12
        || (b[0] == 192 && b[1] == 168);                // 192.168.0.0/16
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Dev convenience only: applies pending migrations + seeds Arabic sample content.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
    await PasswordDatabaseSeeder.SeedAsync(db);
    await RankingDatabaseSeeder.SeedAsync(db);
    await Top100DatabaseSeeder.SeedAsync(db);
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        await Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest)
            .ExecuteAsync(context);
    }
    catch (KeyNotFoundException ex)
    {
        await Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound)
            .ExecuteAsync(context);
    }
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGameSessionEndpoints();
app.MapCategoryEndpoints();
app.MapPasswordGameEndpoints();
app.MapPasswordCategoryEndpoints();
app.MapRevealEndpoints();
app.MapRankingGameEndpoints();
app.MapRankingCategoryEndpoints();
app.MapTop100GameEndpoints();
app.MapTop100CategoryEndpoints();
app.MapAuthEndpoints();
app.MapSessionEndpoints();
app.MapMyCategoryEndpoints();

app.Run();
