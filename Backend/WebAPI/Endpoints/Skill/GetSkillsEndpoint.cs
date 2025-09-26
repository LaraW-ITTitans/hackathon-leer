using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Skill;

public class GetSkillsEndpoint : EndpointWithoutRequest<List<BasicSkillBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetSkillsEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/skills");
        this.AddHackathonPolicy(AuthClaimType.SeeSkill);
        
        this.Description(builder => builder
            .WithName("Get All Skills")
            .WithTags("Skills"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<BasicSkillBindingModel> skills = await this.dbContext.Skills
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new BasicSkillBindingModel
            {
                Id = s.Id,
                Name = s.Name,
            })
            .ToListAsync(ct);

        await this.Send.OkAsync(skills, ct);
    }
}
