using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.DataSource;

public class GetDataSourcesEndpoint : EndpointWithoutRequest<IEnumerable<DataSourceBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetDataSourcesEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/data-sources");
        this.AddHackathonPolicy(AuthClaimType.SeeDataSource);

        this.Description(b => b
            .WithName("GetDataSources")
            .WithTags("DataSources"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<DataSourceEntity> items = await this.dbContext.DataSources
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.Requirements)
            .ThenInclude(r => r.Skill)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        IEnumerable<DataSourceBindingModel> result = items.Select(d => new DataSourceBindingModel
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            RequiredSkills = d.Requirements
                .Select(r => r.Skill)
                .DistinctBy(s => s.Id)
                .Select(s => new BasicSkillBindingModel { Id = s.Id, Name = s.Name })
                .ToList(),
        });

        await this.Send.OkAsync(result, ct);
    }
}