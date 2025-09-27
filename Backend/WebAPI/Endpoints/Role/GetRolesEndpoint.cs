using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Role;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Role;

public class GetRolesEndpoint : EndpointWithoutRequest<List<BasicRoleBindingModel>>
{
    private readonly RoleManager<HackathonRoleEntity> roleManager;

    public GetRolesEndpoint(RoleManager<HackathonRoleEntity> roleManager)
    {
        this.roleManager = roleManager;
    }

    public override void Configure()
    {
        this.Get("api/roles");
        this.AddHackathonPolicy(AuthClaimType.SeeRole);
        
        this.Description(builder => builder
            .WithName("GetAllRoles")
            .WithTags("Roles"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<BasicRoleBindingModel> roles = await this.roleManager.Roles
            .Where(r => !r.IsDeleted)
            .Select(r => new BasicRoleBindingModel
            {
                Id = r.Id,
                Name = r.Name!
            })
            .ToListAsync(ct);

        await this.Send.OkAsync(roles, ct);
    }
}
