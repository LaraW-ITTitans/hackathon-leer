using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class GetUsersEndpoint : EndpointWithoutRequest<List<BasicUserBindingModel>>
{
    private readonly UserManager<HackathonUserEntity> userManager;

    public GetUsersEndpoint(UserManager<HackathonUserEntity> userManager)
    {
        this.userManager = userManager;
    }

    public override void Configure()
    {
        this.Get("api/users");
        this.AddHackathonPolicy(AuthClaimType.SeeUser);
        
        this.Description(builder => builder
            .WithName("GetUsers")
            .WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<BasicUserBindingModel> users = await this.userManager.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new BasicUserBindingModel
            {
                Id = u.Id,
                UserName = u.UserName!,
                DisplayName = u.DisplayName
            })
            .ToListAsync(ct);

        await this.Send.OkAsync(users, ct);
    }
}