using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.DataSource;

public class CreateDataSourceEndpoint : Endpoint<CreateUpdateDataSourceBindingModel, DataSourceBindingModel>
{
    private readonly HackathonDbContext dbContext;
    private readonly ILogger<CreateDataSourceEndpoint> logger;

    public CreateDataSourceEndpoint(HackathonDbContext dbContext, ILogger<CreateDataSourceEndpoint> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Post("api/data-sources");
        this.AddHackathonPolicy(AuthClaimType.ManageDataSource);

        this.Description(b => b
            .WithName("CreateDataSource")
            .WithTags("DataSources"));
    }

    public override async Task HandleAsync(CreateUpdateDataSourceBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        // Validate skill IDs exist and are not deleted
        Guid[] skillIds = req.RequiredSkillIds.Distinct().ToArray();
        if (skillIds.Length > 0)
        {
            int existing = await this.dbContext.Skills.CountAsync(s => skillIds.Contains(s.Id) && !s.IsDeleted, ct);
            if (existing != skillIds.Length)
            {
                await this.HttpContext.Response.StartAsync(ct);
                this.HttpContext.Response.StatusCode = 400;
                await this.HttpContext.Response.WriteAsync("One or more required skills do not exist.", ct);
                return;
            }
        }

        var entity = new DataSourceEntity
        {
            Name = req.Name,
            Description = req.Description,
            IsDeleted = false,
        };

        await this.dbContext.DataSources.AddAsync(entity, ct);
        await this.dbContext.SaveChangesAsync(ct);

        if (skillIds.Length > 0)
        {
            IEnumerable<DataSourceRequirementEntity> requirements = skillIds.Select(id => new DataSourceRequirementEntity
            {
                DataSourceId = entity.Id,
                SkillId = id
            });
            await this.dbContext.DataSourceRequirements.AddRangeAsync(requirements, ct);
            await this.dbContext.SaveChangesAsync(ct);
        }

        this.logger.LogInformation("Created data source {Name}", req.Name);

        // Load with requirements for response
        DataSourceEntity full = await this.dbContext.DataSources
            .AsNoTracking()
            .Where(d => d.Id == entity.Id)
            .Include(d => d.Requirements)
            .ThenInclude(r => r.Skill)
            .FirstAsync(ct);

        await this.Send.OkAsync(new DataSourceBindingModel
        {
            Id = full.Id,
            Name = full.Name,
            Description = full.Description,
            RequiredSkills = full.Requirements
                .Select(r => r.Skill)
                .DistinctBy(s => s.Id)
                .Select(s => new BasicSkillBindingModel { Id = s.Id, Name = s.Name })
                .ToList(),
        }, ct);
    }
}