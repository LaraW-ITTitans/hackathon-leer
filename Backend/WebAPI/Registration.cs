using ITTitans.Hackathon2025.Service;
using ITTitans.Hackathon2025.Service.Interfaces;
using ITTitans.Hackathon2025.Service.Interfaces.Settings;
using ITTitans.Hackathon2025.Service.Settings;
using ITTitans.Hackathon2025.WebAPI.Auth;
using Microsoft.AspNetCore.Authorization;

namespace ITTitans.Hackathon2025.WebAPI;

public static class Registration
{
    public static IServiceCollection AddDependencyInjectionRegistrations(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<IEnsureCreatedPredefinedEntitiesService, EnsureCreatedPredefinedEntitiesService>()
            .AddScoped<IAppSettingsReader, AppSettingsReader>()
            .AddScoped<IWebServerAppSettingsService, WebServerAppSettingsService>()
            .AddScoped<IAuthClaimFactory, AuthClaimFactory>()
            .AddScoped<IJwtWebApiService, JwtWebApiService>()
            .AddSingleton<IAuthorizationHandler, OrClaimsRequirementAuthorizationHandler>();;
    }
}