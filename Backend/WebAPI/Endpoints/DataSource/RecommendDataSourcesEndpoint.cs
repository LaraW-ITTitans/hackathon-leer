using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.Service.Interfaces.Settings;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.DataSource;

public class RecommendDataSourcesRequest
{
    public string Query { get; set; } = string.Empty;
    public int Max { get; set; } = 10;
}

public class RecommendDataSourcesEndpoint : Endpoint<RecommendDataSourcesRequest, IEnumerable<DataSourceRecommendationBindingModel>>
{
    private readonly HackathonDbContext hackathonDbContext;
    private readonly IWebServerAppSettingsService webServerAppSettingsService;

    public RecommendDataSourcesEndpoint(HackathonDbContext hackathonDbContext, IWebServerAppSettingsService webServerAppSettingsService)
    {
        this.hackathonDbContext = hackathonDbContext;
        this.webServerAppSettingsService = webServerAppSettingsService;
    }

    public override void Configure()
    {
        // GET api/data-sources/recommend?query=...&max=10
        this.Get("api/data-sources/recommend");
        // No explicit policy; requires authentication via claim extraction
        this.Description(b => b
            .WithName("RecommendDataSources")
            .WithTags("DataSources"));
    }

    public override async Task HandleAsync(RecommendDataSourcesRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        
        if (string.IsNullOrWhiteSpace(req.Query))
        {
            this.AddError("Missing query parameter 'query'.");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        int max = req.Max > 0 ? req.Max : 10;

        // Authentication: need current user id to determine skill access
        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }

        // Load user's skills
        HashSet<Guid> userSkills = (await this.hackathonDbContext.SkillAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.SkillId)
            .ToListAsync(ct)).ToHashSet();

        // Load all data sources with requirements and skill entities (non-deleted)
        List<DataSourceEntity> dataSources = await this.hackathonDbContext.DataSources
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.Requirements)
            .ThenInclude(r => r.Skill)
            .ToListAsync(ct);

        if (dataSources.Count == 0)
        {
            await this.Send.OkAsync([], ct);
            return;
        }

        List<DataSourceRecommendationBindingModel> response = null!; // TODO
        
        string openAiApiKey = this.webServerAppSettingsService.GetOpenAiApiKey();

        await this.Send.OkAsync(response, ct);
    }

    private static string Combine(string name, string? description)
        => string.IsNullOrWhiteSpace(description) ? name : $"{name} {description}";

    private sealed class TextDoc
    {
        public string Text { get; set; } = string.Empty;
    }
}
