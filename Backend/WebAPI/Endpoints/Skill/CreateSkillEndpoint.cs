using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Utils;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Skill;

public class CreateSkillEndpoint : Endpoint<CreateSkillBindingModel, BasicSkillBindingModel>
{
    private readonly HackathonDbContext dbContext;
    private readonly ILogger<CreateSkillEndpoint> logger;

    public CreateSkillEndpoint(HackathonDbContext dbContext, ILogger<CreateSkillEndpoint> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Post("api/skills");
        this.AddHackathonPolicy(AuthClaimType.ManageSkill);

        this.Description(builder => builder
            .WithName("CreateSkill")
            .WithTags("Skills"));
    }

    public override async Task HandleAsync(CreateSkillBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var entity = new SkillEntity
        {
            Name = req.Name,
            Description = req.Description,
            IsDeleted = false,
        };

        await this.dbContext.Skills.AddAsync(entity, ct);
        await this.dbContext.SaveChangesAsync(ct);

        this.logger.LogInformation("Created skill {Name}", req.Name);

        await this.Send.OkAsync(new BasicSkillBindingModel
        {
            Id = entity.Id,
            Name = entity.Name,
        }, ct);
    }
}
