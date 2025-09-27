using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Role;

public class DeleteRoleEndpoint : EndpointWithoutRequest
{
    private readonly RoleManager<HackathonRoleEntity> roleManager;
    private readonly ILogger<DeleteRoleEndpoint> logger;

    public DeleteRoleEndpoint(RoleManager<HackathonRoleEntity> roleManager, ILogger<DeleteRoleEndpoint> logger)
    {
        this.roleManager = roleManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Delete("api/roles/{id:guid}");
        this.AddHackathonPolicy(AuthClaimType.ManageRole);
        
        this.Description(builder => builder
            .WithName("DeleteRole")
            .WithTags("Roles"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = this.Route<Guid>("id");

        HackathonRoleEntity? role = await this.roleManager.FindByIdAsync(id.ToString());
        if (role is null || role.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        role.IsDeleted = true;
        IdentityResult result = await this.roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.AddError($"{error.Code}: {error.Description}");
            }

            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        this.logger.LogInformation("Deleted role {Id}", id);
        await this.Send.OkAsync(cancellation: ct);
    }
}
