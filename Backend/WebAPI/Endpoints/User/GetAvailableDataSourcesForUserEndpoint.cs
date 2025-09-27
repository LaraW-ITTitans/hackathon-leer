using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.Model.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class GetAvailableDataSourcesForUserEndpoint : EndpointWithoutRequest<IEnumerable<DataSourceBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetAvailableDataSourcesForUserEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/users/{id:guid}/available-datasources");
        // No global policy: authorization is enforced in handler (self or SeeUser)
        this.Description(b => b
            .WithName("GetAvailableDataSourcesForUser")
            .WithTags("Users", "DataSources"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var targetUserId = this.Route<Guid>("id");

        string? currentUserIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        bool isAuthenticated = Guid.TryParse(currentUserIdClaim, out Guid currentUserId);

        bool isSelf = isAuthenticated && currentUserId == targetUserId;
        bool hasSeeUser = this.User.HasClaim(HackathonClaims.AuthClaimName, nameof(AuthClaimType.SeeUser));

        if (!isSelf && !hasSeeUser)
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 403;
            return;
        }

        // Gather user's assigned skills
        HashSet<Guid> userSkills = (await this.dbContext.SkillAssignments
            .AsNoTracking()
            .Where(a => a.UserId == targetUserId)
            .Select(a => a.SkillId)
            .ToListAsync(ct)).ToHashSet();

        // Get all non-deleted data sources with their requirements
        List<DataSourceEntity> dataSources = await this.dbContext.DataSources
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.Requirements)
            .ThenInclude(r => r.Skill)
            .ToListAsync(ct);

        IEnumerable<DataSourceBindingModel> available = dataSources
            .Where(d => d.Requirements.Select(r => r.SkillId).All(req => userSkills.Contains(req)))
            .Select(d => new DataSourceBindingModel
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

        await this.Send.OkAsync(available, ct);
    }
}