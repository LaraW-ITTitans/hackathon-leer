using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class UpdateUserEndpoint : Endpoint<UpdateUserBindingModel, BasicUserBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly ILogger<UpdateUserEndpoint> logger;

    public UpdateUserEndpoint(UserManager<HackathonUserEntity> userManager, ILogger<UpdateUserEndpoint> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Put("api/users");
        this.AddHackathonPolicy(AuthClaimType.ManageUser);
        
        this.Description(builder => builder
            .WithName("UpdateUser")
            .WithTags("Users"));
    }

    public override async Task HandleAsync(UpdateUserBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        HackathonUserEntity? user = await this.userManager.FindByIdAsync(req.Id.ToString());
        if (user is null || user.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        user.UserName = req.UserName;
        user.DisplayName = req.DisplayName;
        IdentityResult updateResult = await this.userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (IdentityError error in updateResult.Errors)
            {
                this.AddError($"{error.Code}: {error.Description}");
            }

            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            string token = await this.userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult pwdResult = await this.userManager.ResetPasswordAsync(user, token, req.Password);
            if (!pwdResult.Succeeded)
            {
                foreach (IdentityError error in pwdResult.Errors)
                {
                    this.AddError($"{error.Code}: {error.Description}");
                }

                await this.Send.ErrorsAsync(cancellation: ct);
                return;
            }
        }

        this.logger.LogInformation("Updated user {Id}", req.Id);

        await this.Send.OkAsync(new BasicUserBindingModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            DisplayName = user.DisplayName,
        }, ct);
    }
}