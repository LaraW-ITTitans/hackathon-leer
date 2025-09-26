using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class ChangeOwnPasswordEndpoint : Endpoint<ChangeOwnPasswordBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly ILogger<ChangeOwnPasswordEndpoint> logger;

    public ChangeOwnPasswordEndpoint(UserManager<HackathonUserEntity> userManager, ILogger<ChangeOwnPasswordEndpoint> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Put("api/users/change-password");
        // No special claim required; any authenticated user can call this.
        // Authorization is enforced by requiring presence of our user id claim at runtime.
    }

    public override async Task HandleAsync(ChangeOwnPasswordBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        string? userIdStr = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdStr, out Guid userId))
        {
            // not authenticated or invalid token
            this.HttpContext.Response.StatusCode = 401;
            await this.HttpContext.Response.CompleteAsync();
            return;
        }

        HackathonUserEntity? user = await this.userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        bool passwordOk = await this.userManager.CheckPasswordAsync(user, req.CurrentPassword);
        if (!passwordOk)
        {
            this.AddError("Invalid current password");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        IdentityResult result = await this.userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.AddError($"{error.Code}: {error.Description}");
            }

            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        this.logger.LogInformation("Changed password for user {UserId}", userId);
        await this.Send.OkAsync(cancellation: ct);
    }
}