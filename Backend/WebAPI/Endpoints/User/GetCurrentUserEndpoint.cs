using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class GetCurrentUserEndpoint : EndpointWithoutRequest<BasicUserBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;

    public GetCurrentUserEndpoint(UserManager<HackathonUserEntity> userManager)
    {
        this.userManager = userManager;
    }

    public override void Configure()
    {
        this.Get("api/users/me");
        this.AllowAnonymous();
        
        this.Description(builder => builder
            .WithName("GetCurrentUser")
            .WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            // not authenticated or invalid token
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }

        HackathonUserEntity? user = await this.userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        await this.Send.OkAsync(new BasicUserBindingModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            DisplayName = user.DisplayName
        }, ct);
    }
}
