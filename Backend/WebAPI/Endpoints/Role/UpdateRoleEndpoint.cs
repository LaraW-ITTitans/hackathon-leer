using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Role;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Role;

public class UpdateRoleEndpoint : Endpoint<UpdateRoleBindingModel, BasicRoleBindingModel>
{
    private readonly RoleManager<HackathonRoleEntity> roleManager;
    private readonly ILogger<UpdateRoleEndpoint> logger;

    public UpdateRoleEndpoint(RoleManager<HackathonRoleEntity> roleManager, ILogger<UpdateRoleEndpoint> logger)
    {
        this.roleManager = roleManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Put("api/roles");
        this.AddHackathonPolicy(AuthClaimType.ManageRole);
        
        this.Description(builder => builder
            .WithName("UpdateRole")
            .WithTags("Roles"));
    }

    public override async Task HandleAsync(UpdateRoleBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        HackathonRoleEntity? role = await this.roleManager.FindByIdAsync(req.Id.ToString());
        if (role is null || role.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        role.Name = req.Name;
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

        this.logger.LogInformation("Updated role {Id} name to {Name}", req.Id, req.Name);

        await this.Send.OkAsync(new BasicRoleBindingModel
        {
            Id = role.Id,
            Name = role.Name!
        }, ct);
    }
}
