using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Utils;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class GetCurrentUserEndpoint : EndpointWithoutRequest<DetailedUserBindingModel>
{
    private readonly UserManager<HackathonUserEntity> userManager;
    private readonly HackathonDbContext hackathonDbContext;

    public GetCurrentUserEndpoint(UserManager<HackathonUserEntity> userManager, HackathonDbContext hackathonDbContext)
    {
        this.userManager = userManager;
        this.hackathonDbContext = hackathonDbContext;
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
            await this.Send.ForbiddenAsync(ct);
            return;
        }

        HackathonUserEntity? user = await this.userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }
        
        List<BasicSkillBindingModel> assignedSkills = await this.hackathonDbContext.SkillAssignments
            .Where(x => x.UserId == user.Id)
            .Select(x => new BasicSkillBindingModel
            {
                Id = x.Skill.Id,
                Name = x.Skill.Name,
            })
            .ToListAsync(ct);

        List<BasicSkillBindingModel> reviewableSkills;
        if (user.Id == StaticData.PredefinedAdminUserId)
        {
            reviewableSkills = await this.hackathonDbContext.Skills
                .Where(x => !x.IsDeleted)
                .Select(x => new BasicSkillBindingModel
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .ToListAsync(ct);
        }
        else
        {
            reviewableSkills = await this.hackathonDbContext.SkillReviewConfigurations
                .Where(x => x.UserId == user.Id)
                .Select(x => new BasicSkillBindingModel
                {
                    Id = x.Skill.Id,
                    Name = x.Skill.Name,
                })
                .ToListAsync(ct);
        }
        
        var result = new DetailedUserBindingModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            DisplayName = user.DisplayName,
            Skills = assignedSkills,
            ReviewableSkills = reviewableSkills,
        };
        
        await this.Send.OkAsync(result, ct);
    }
}
