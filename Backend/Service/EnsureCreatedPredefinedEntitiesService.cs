using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Service.Interfaces;
using ITTitans.Hackathon2025.Service.Interfaces.Settings;
using ITTitans.Hackathon2025.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.Service;

public class EnsureCreatedPredefinedEntitiesService : IEnsureCreatedPredefinedEntitiesService
{
    private readonly IWebServerAppSettingsService webServerAppSettingsService;
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly RoleManager<HackathonRoleEntity> roleManager;
    private readonly IAuthClaimFactory authClaimFactory;
    private readonly ILogger<EnsureCreatedPredefinedEntitiesService> logger;

    public EnsureCreatedPredefinedEntitiesService(IWebServerAppSettingsService webServerAppSettingsService, UserManager<HackathonUserEntity> userManager, RoleManager<HackathonRoleEntity> roleManager, IAuthClaimFactory authClaimFactory, ILogger<EnsureCreatedPredefinedEntitiesService> logger)
    {
        this.webServerAppSettingsService = webServerAppSettingsService;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.authClaimFactory = authClaimFactory;
        this.logger = logger;
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        // ensure created predefined admin role
        HackathonRoleEntity? predefinedAdminRole = await this.roleManager.FindByIdAsync(
            StaticData.PredefinedAdminRoleId.ToString()
        );
        if (predefinedAdminRole is null)
        {
            predefinedAdminRole = new HackathonRoleEntity
            {
                Id = StaticData.PredefinedAdminRoleId,
                Name = StaticData.PredefinedAdminRoleName,
                IsDeleted = false,
            };
            IdentityResult identityResult = await this.roleManager.CreateAsync(predefinedAdminRole);
            if (!identityResult.Succeeded)
            {
                this.logger.LogCannotCreatePredefinedAdminRole(
                    identityResult.Errors.FirstOrDefault()?.Description ?? "unknown"
                );
                return;
            }
        }

        // ensure assigned claims to admin role
        AuthClaimType[] allSiGeKoAuthClaimTypes = Enum.GetValues<AuthClaimType>();

        IList<Claim> assignedClaims = await this.roleManager.GetClaimsAsync(predefinedAdminRole);
        IList<AuthClaimType> assignedSiGeKoAuthClaims = assignedClaims
            .Select(this.authClaimFactory.Extract)
            .Where(nullableSiGeKoAuthClaim => nullableSiGeKoAuthClaim.HasValue)
            .Select(nullableSiGeKoAuthClaim => nullableSiGeKoAuthClaim!.Value)
            .ToList();
        IEnumerable<AuthClaimType> unassignedSiGeKoAuthClaims =
            allSiGeKoAuthClaimTypes.Except(assignedSiGeKoAuthClaims);

        foreach (AuthClaimType authClaimType in unassignedSiGeKoAuthClaims)
        {
            Claim claim = this.authClaimFactory.BuildClaim(authClaimType);

            IdentityResult identityResult = await this.roleManager.AddClaimAsync(
                predefinedAdminRole,
                claim
            );
            if (!identityResult.Succeeded)
            {
                this.logger.LogCannotAssignAuthClaimToAdminRole(
                    authClaimType,
                    identityResult.Errors.FirstOrDefault()?.Description ?? "unknown"
                );
            }
        }

        // ensure created predefined admin user
        HackathonUserEntity? predefinedAdminUser = await this.userManager.FindByIdAsync(
            StaticData.PredefinedAdminUserId.ToString()
        );
        if (predefinedAdminUser is null)
        {
            predefinedAdminUser = new HackathonUserEntity
            {
                Id = StaticData.PredefinedAdminUserId,
                UserName = StaticData.PredefinedAdminUserName,
                DisplayName = "Admin",
                IsDeleted = false,
            };
            IdentityResult identityResult = await this.userManager.CreateAsync(predefinedAdminUser);
            if (!identityResult.Succeeded)
            {
                this.logger.LogCannotCreatePredefinedAdminUser(
                    identityResult.Errors.FirstOrDefault()?.Description ?? "unknown"
                );
                return;
            }
        }

        // set password of predefined admin user
        if (await this.userManager.HasPasswordAsync(predefinedAdminUser))
        {
            IdentityResult identityResult = await this.userManager.RemovePasswordAsync(
                predefinedAdminUser
            );
            if (!identityResult.Succeeded)
            {
                this.logger.LogCannotCreatePredefinedAdminUser(
                    identityResult.Errors.FirstOrDefault()?.Description ?? "unknown"
                );
                return;
            }
        }

        string passwordOfPredefinedAdminUser =
            this.webServerAppSettingsService.GetPredefinedAdminUserPassword();
        IdentityResult addPasswordIdentityResult = await this.userManager.AddPasswordAsync(
            predefinedAdminUser,
            passwordOfPredefinedAdminUser
        );
        if (!addPasswordIdentityResult.Succeeded)
        {
            this.logger.LogCannotAddPasswordForPredefinedAdminUser(
                addPasswordIdentityResult.Errors.FirstOrDefault()?.Description ?? "unknown"
            );
            return;
        }

        if (
            !await this.userManager.IsInRoleAsync(
                predefinedAdminUser,
                StaticData.PredefinedAdminRoleName
            )
        )
        {
            IdentityResult assignAdminRoleIdentityResult = await this.userManager.AddToRoleAsync(
                predefinedAdminUser,
                StaticData.PredefinedAdminRoleName
            );
            if (!assignAdminRoleIdentityResult.Succeeded)
            {
                this.logger.LogCannotAssignAdminRoleToAdminUser(
                    addPasswordIdentityResult.Errors.FirstOrDefault()?.Description ?? "unknown"
                );
            }
        }
    }
}

internal static partial class LoggerMessageDefinitions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Critical,
        Message = "Cannot create predefined admin role (first error: {firstError})"
    )]
    public static partial void LogCannotCreatePredefinedAdminRole(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        string firstError
    );

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Critical,
        Message = "Cannot create predefined admin user (first error: {firstError})"
    )]
    public static partial void LogCannotCreatePredefinedAdminUser(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        string firstError
    );

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Critical,
        Message = "Cannot remove password of predefined admin user (first error: {firstError})"
    )]
    public static partial void LogCannotRemovePasswordOfPredefinedAdminUser(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        string firstError
    );

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Critical,
        Message = "Cannot add password for predefined admin user (first error: {firstError})"
    )]
    public static partial void LogCannotAddPasswordForPredefinedAdminUser(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        string firstError
    );

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Initialized predefined admin user and role"
    )]
    public static partial void LogSuccessfulPredefinedEntities(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger
    );

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Critical,
        Message = "Cannot assign admin role to admin user (first error: {firstError})"
    )]
    public static partial void LogCannotAssignAdminRoleToAdminUser(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        string firstError
    );

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Critical,
        Message = "Cannot assign auth claim {authClaimType} to admin role (first error: {firstError})"
    )]
    public static partial void LogCannotAssignAuthClaimToAdminRole(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        AuthClaimType authClaimType,
        string firstError
    );

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Critical,
        Message = "Cannot create predefined reporter role (first error: {firstError})"
    )]
    public static partial void LogCannotCreatePredefinedReporterRole(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        string firstError
    );

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Critical,
        Message = "Cannot assign auth claim {authClaimType} to reporter role (first error: {firstError})"
    )]
    public static partial void LogCannotAssignAuthClaimToReporterRole(
        this ILogger<EnsureCreatedPredefinedEntitiesService> logger,
        AuthClaimType authClaimType,
        string firstError
    );
}