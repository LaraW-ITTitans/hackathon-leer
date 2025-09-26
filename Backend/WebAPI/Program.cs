using FastEndpoints;
using FastEndpoints.Swagger;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Service.Interfaces;
using ITTitans.Hackathon2025.Service.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

namespace ITTitans.Hackathon2025.WebAPI;

public static class Program
{
    private const string JwtHttpHeader = "X-Hackathon-Token";
    private const string CorsConfigurationName = "CorsConfiguration";
    
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        ConfigureService(builder);

        WebApplication app = builder.Build();
        
        ConfigureApp(app);

        using (IServiceScope serviceScope = app.Services.CreateScope())
        {
            await EnsureDatabaseCreatedAsync(serviceScope);
        }

        await app.RunAsync();
    }

    private static void ConfigureService(WebApplicationBuilder builder)
    {
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

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(swaggerGenerationOptions =>
            {
                swaggerGenerationOptions.SwaggerDoc(
                    "v1",
                    new OpenApiInfo { Title = "Hackathon API", Version = "v1" }
                );

                // enforce using JWT
                swaggerGenerationOptions.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Scheme = "bearer",
                        Description = "SiGeKo JWT",
                        Name = Program.JwtHttpHeader,
                    }
                );
                swaggerGenerationOptions.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                            },
                            Array.Empty<string>()
                        },
                    }
                );

                // integrate documentation
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swaggerGenerationOptions.IncludeXmlComments(xmlPath);
            });
        }
        
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

        JwtAppSettingsDto jwtSettings = webServerAppSettingsService.GetJwtSettings();
        builder.Services
            .AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                jwtBearerOptions =>
                {
                    string keyAsString = jwtSettings.Key;
                    byte[] key = Encoding.UTF8.GetBytes(keyAsString);

                    jwtBearerOptions.SaveToken = true;
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };

                    jwtBearerOptions.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (
                                context.Request.Headers.TryGetValue(
                                    Program.JwtHttpHeader,
                                    out StringValues value
                                )
                            )
                            {
                                context.Token = value;
                            }

                            return Task.CompletedTask;
                        },
                    };
                }
            );
        builder.Services.AddHackathonPolicies();

        builder.Services.AddAuthorization();
    }

    private static void ConfigureApp(WebApplication app)
    {
        app.UseCors(Program.CorsConfigurationName);
        
        app.UseHttpsRedirection();
        app.UseRouting();
        
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
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