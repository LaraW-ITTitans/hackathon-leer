using FastEndpoints;
using FastEndpoints.Swagger;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Service.Interfaces;
using ITTitans.Hackathon2025.Service.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI;

public static class Program
{
    private const string CorsConfigurationName = "CorsConfiguration";
    
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        var webServerAppSettingsService = new WebServerAppSettingsService(new AppSettingsReader(builder.Configuration));
        
        builder.Services.AddFastEndpoints();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: Program.CorsConfigurationName,
                corsPolicyBuilder =>
                {
                    // frontend
                    string frontendUriAsString = webServerAppSettingsService.GetFrontendUri().OriginalString;
                    corsPolicyBuilder
                        .WithOrigins(frontendUriAsString)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition");
                }
            );
        });
        
        builder.Services.SwaggerDocument();
        
        string connectionString = webServerAppSettingsService.GetHackathonDbContextConnectionString();
        builder.Services.AddDbContext<HackathonDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        builder.Services
            .AddIdentityCore<HackathonUserEntity>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<HackathonRoleEntity>()
            .AddEntityFrameworkStores<HackathonDbContext>()
            .AddSignInManager();

        builder.Services.AddDependencyInjectionRegistrations();

        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme);

        builder.Services.AddAuthorization();

        WebApplication app = builder.Build();
        
        app.UseCors(Program.CorsConfigurationName);
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerGen();
        }

        using (IServiceScope serviceScope = app.Services.CreateScope())
        {
            await EnsureDatabaseCreatedAsync(serviceScope);
        }

        await app.RunAsync();
    }

    private static async Task EnsureDatabaseCreatedAsync(
        IServiceScope serviceScope,
        CancellationToken cancellationToken = default)
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<HackathonDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        
        var ensureCreatedPredefinedEntitiesService = serviceScope.ServiceProvider.GetRequiredService<IEnsureCreatedPredefinedEntitiesService>();
        await ensureCreatedPredefinedEntitiesService.EnsureCreatedAsync(cancellationToken);
    }
}