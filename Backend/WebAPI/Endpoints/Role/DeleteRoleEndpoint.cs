using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Role;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Role;

public class DeleteRoleEndpoint : Endpoint<DeleteRoleBindingModel>
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
        this.Delete("api/roles");
        this.AddHackathonPolicy(AuthClaimType.ManageRole);
        
        this.Description(builder => builder
            .WithName("DeleteRole")
            .WithTags("Roles"));
    }

    public override async Task HandleAsync(DeleteRoleBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        HackathonRoleEntity? role = await this.roleManager.FindByIdAsync(req.Id.ToString());
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

        this.logger.LogInformation("Deleted role {Id}", req.Id);
        await this.Send.OkAsync(cancellation: ct);
    }
}
