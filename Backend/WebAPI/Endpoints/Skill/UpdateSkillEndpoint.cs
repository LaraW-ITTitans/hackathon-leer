using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Skill;

public class UpdateSkillEndpoint : Endpoint<UpdateSkillBindingModel, BasicSkillBindingModel>
{
    private readonly HackathonDbContext dbContext;
    private readonly ILogger<UpdateSkillEndpoint> logger;

    public UpdateSkillEndpoint(HackathonDbContext dbContext, ILogger<UpdateSkillEndpoint> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Put("api/skills");
        this.AddHackathonPolicy(AuthClaimType.ManageSkill);

        this.Description(builder => builder
            .WithName("UpdateSkill")
            .WithTags("Skills"));
    }

    public override async Task HandleAsync(UpdateSkillBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        SkillEntity? skill = await this.dbContext.Skills.FirstOrDefaultAsync(s => s.Id == req.Id && !s.IsDeleted, ct);
        if (skill is null)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        skill.Name = req.Name;
        skill.Description = req.Description;
        await this.dbContext.SaveChangesAsync(ct);

        this.logger.LogInformation("Updated skill {Id}", req.Id);

        await this.Send.OkAsync(new BasicSkillBindingModel
        {
            Id = skill.Id,
            Name = skill.Name,
        }, ct);
    }
}
