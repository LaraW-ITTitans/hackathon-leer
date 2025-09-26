using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Role;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Role;

public class CreateRoleEndpoint : Endpoint<CreateRoleBindingModel, BasicRoleBindingModel>
{
    private readonly RoleManager<HackathonRoleEntity> roleManager;
    private readonly ILogger<CreateRoleEndpoint> logger;

    public CreateRoleEndpoint(RoleManager<HackathonRoleEntity> roleManager, ILogger<CreateRoleEndpoint> logger)
    {
        this.roleManager = roleManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Post("api/roles");
        this.AddHackathonPolicy(AuthClaimType.ManageRole);
    }

    public override async Task HandleAsync(CreateRoleBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        
        var role = new HackathonRoleEntity
        {
            Name = req.Name,
            IsDeleted = false,
        };
        IdentityResult result = await this.roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                this.AddError($"{error.Code}: {error.Description}");
            }
            
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }
        
        this.logger.LogInformation("Created role {Name}", req.Name);
        
        await this.Send.OkAsync(new BasicRoleBindingModel
        {
            Id = role.Id,
            Name = role.Name,
        }, ct);
    }
}