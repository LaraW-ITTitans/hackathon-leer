using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class CreateUserEndpoint : Endpoint<CreateUserBindingModel, BasicUserBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly ILogger<CreateUserEndpoint> logger;

    public CreateUserEndpoint(UserManager<HackathonUserEntity> userManager, ILogger<CreateUserEndpoint> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Post("api/users");
        this.AddHackathonPolicy(AuthClaimType.ManageUser);
    }

    public override async Task HandleAsync(CreateUserBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var user = new HackathonUserEntity
        {
            UserName = req.UserName,
            DisplayName = req.DisplayName,
            IsDeleted = false,
        };

        IdentityResult result = await this.userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.AddError($"{error.Code}: {error.Description}");
            }

            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        this.logger.LogInformation("Created user {UserName}", req.UserName);

        await this.Send.OkAsync(new BasicUserBindingModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            DisplayName = user.DisplayName,
        }, ct);
    }
}