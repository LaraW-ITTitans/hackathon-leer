using FastEndpoints;
using FastEndpoints.Swagger;
using ITTitans.Hackathon2025.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Explicitly configure configuration sources: appsettings.json, environment-specific json, and environment variables
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Add services to the container.
        builder.Services.AddFastEndpoints();
        builder.Services.SwaggerDocument();

        // Configure EF Core DbContext with PostgreSQL (Npgsql)
        string connectionString = builder.Configuration.GetConnectionString("HackathonDbContext")
                                  ?? throw new InvalidOperationException("ConnectionStrings:HackathonDbContext is not configured.");
        builder.Services.AddDbContext<HackathonDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Configure ASP.NET Core Identity with EF Core stores (lean setup for APIs)
        builder.Services
            .AddIdentityCore<HackathonUserEntity>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<HackathonRoleEntity>()
            .AddEntityFrameworkStores<HackathonDbContext>()
            .AddSignInManager();

        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        builder.Services.AddAuthorization();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // Swagger is configured via FastEndpoints.Swagger
            // builder.Services.SwaggerDocument() + app.UseSwaggerGen()
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints();
        app.UseSwaggerGen();

        await EnsureDatabaseCreatedAsync(app);

        await app.RunAsync();
    }

    private static async Task EnsureDatabaseCreatedAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        IServiceScope serviceScope = app.Services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<HackathonDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}