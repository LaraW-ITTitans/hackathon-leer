using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.DataSource;

public class UpdateDataSourceEndpoint : Endpoint<CreateUpdateDataSourceBindingModel, DataSourceBindingModel>
{
    private readonly HackathonDbContext dbContext;
    private readonly ILogger<UpdateDataSourceEndpoint> logger;

    public UpdateDataSourceEndpoint(HackathonDbContext dbContext, ILogger<UpdateDataSourceEndpoint> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Put("api/data-sources/{id:guid}");
        this.AddHackathonPolicy(AuthClaimType.ManageDataSource);

        this.Description(b => b
            .WithName("UpdateDataSource")
            .WithTags("DataSources"));
    }

    public override async Task HandleAsync(CreateUpdateDataSourceBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        var id = this.Route<Guid>("id");

        DataSourceEntity? entity = await this.dbContext.DataSources
            .Include(d => d.Requirements)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);

        if (entity is null)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

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

        entity.Name = req.Name;
        entity.Description = req.Description;

        // Update requirements (replace set)
        HashSet<Guid> current = entity.Requirements.Select(r => r.SkillId).ToHashSet();
        HashSet<Guid> desired = skillIds.ToHashSet();

        List<DataSourceRequirementEntity> toRemove = entity.Requirements.Where(r => !desired.Contains(r.SkillId)).ToList();
        if (toRemove.Count > 0)
        {
            this.dbContext.DataSourceRequirements.RemoveRange(toRemove);
        }

        List<DataSourceRequirementEntity> toAdd = desired.Except(current)
            .Select(skillId => new DataSourceRequirementEntity { DataSourceId = entity.Id, SkillId = skillId })
            .ToList();
        if (toAdd.Count > 0)
        {
            await this.dbContext.DataSourceRequirements.AddRangeAsync(toAdd, ct);
        }

        await this.dbContext.SaveChangesAsync(ct);

        // Reload for response
        DataSourceEntity full = await this.dbContext.DataSources
            .AsNoTracking()
            .Where(d => d.Id == entity.Id)
            .Include(d => d.Requirements)
            .ThenInclude(r => r.Skill)
            .FirstAsync(ct);

        this.logger.LogInformation("Updated data source {Id}", id);

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