using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Skill;

public class DeleteSkillEndpoint : EndpointWithoutRequest
{
    private readonly HackathonDbContext dbContext;
    private readonly ILogger<DeleteSkillEndpoint> logger;

    public DeleteSkillEndpoint(HackathonDbContext dbContext, ILogger<DeleteSkillEndpoint> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Delete("api/skills/{id:guid}");
        this.AddHackathonPolicy(AuthClaimType.ManageSkill);

        this.Description(builder => builder
            .WithName("DeleteSkill")
            .WithTags("Skills"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = this.Route<Guid>("id");

        SkillEntity? skill = await this.dbContext.Skills.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
        if (skill is null)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        skill.IsDeleted = true;
        await this.dbContext.SaveChangesAsync(ct);

        this.logger.LogInformation("Deleted skill {Id}", id);

        await this.Send.OkAsync(cancellation: ct);
    }
}
