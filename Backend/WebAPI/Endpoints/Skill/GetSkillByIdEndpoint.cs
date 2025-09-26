using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Skill;

public class GetSkillByIdEndpoint : EndpointWithoutRequest<SkillDetailBindingModel>
{
    private readonly HackathonDbContext dbContext;

    public GetSkillByIdEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/skills/{id:guid}");
        this.AddHackathonPolicy(AuthClaimType.SeeSkill);
        
        this.Description(builder => builder
            .WithName("Get Skill By Id")
            .WithTags("Skills"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = this.Route<Guid>("id");

        SkillDetailBindingModel? skill = await this.dbContext.Skills
            .Include(s => s.PossibleReviewer)
            .Where(s => s.Id == id && !s.IsDeleted)
            .Select(s => new SkillDetailBindingModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                PossibleReviewers = s.PossibleReviewer
                    .Where(u => !u.IsDeleted)
                    .Select(u => new BasicUserBindingModel
                    {
                        Id = u.Id,
                        UserName = u.UserName!,
                        DisplayName = u.DisplayName,
                    })
            })
            .FirstOrDefaultAsync(ct);

        if (skill is null)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        await this.Send.OkAsync(skill, ct);
    }
}
