using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class DeleteUserEndpoint : Endpoint<DeleteUserBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly ILogger<DeleteUserEndpoint> logger;

    public DeleteUserEndpoint(UserManager<HackathonUserEntity> userManager, ILogger<DeleteUserEndpoint> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Delete("api/users");
        this.AddHackathonPolicy(AuthClaimType.ManageUser);
    }

    public override async Task HandleAsync(DeleteUserBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        HackathonUserEntity? user = await this.userManager.FindByIdAsync(req.Id.ToString());
        if (user is null || user.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        user.IsDeleted = true;
        IdentityResult result = await this.userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.AddError($"{error.Code}: {error.Description}");
            }

            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        this.logger.LogInformation("Deleted user {Id}", req.Id);
        await this.Send.OkAsync(cancellation: ct);
    }
}