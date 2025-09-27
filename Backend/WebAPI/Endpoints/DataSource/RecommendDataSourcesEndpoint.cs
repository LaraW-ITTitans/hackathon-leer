using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.DataSource;

public class RecommendDataSourcesRequest
{
    public string Query { get; set; } = string.Empty;
    public int Max { get; set; } = 10;
}

public class RecommendDataSourcesEndpoint : Endpoint<RecommendDataSourcesRequest, IEnumerable<DataSourceRecommendationBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public RecommendDataSourcesEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
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
        HashSet<Guid> userSkills = (await this.dbContext.SkillAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.SkillId)
            .ToListAsync(ct)).ToHashSet();

        // Load all data sources with requirements and skill entities (non-deleted)
        List<DataSourceEntity> dataSources = await this.dbContext.DataSources
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

        // Build ML.NET text featurization and compute cosine similarity between query and each DS text
        var ml = new MLContext(seed: 42);

        // Prepare in-memory data
        List<TextDoc> data = dataSources.Select(d => new TextDoc { Text = Combine(d.Name, d.Description) }).ToList();
        IDataView dataView = ml.Data.LoadFromEnumerable(data);

        TextFeaturizingEstimator? pipeline = ml.Transforms.Text.FeaturizeText("Features", nameof(TextDoc.Text));
        ITransformer model = pipeline.Fit(dataView);

        // Transform dataset to get features
        IDataView transformed = model.Transform(dataView);
        VBuffer<float>[] featuresColumn = transformed.GetColumn<VBuffer<float>>("Features").ToArray();

        // Transform query to feature vector
        IDataView queryView = ml.Data.LoadFromEnumerable([new TextDoc { Text = req.Query }]);
        VBuffer<float> queryFeatures = model.Transform(queryView).GetColumn<VBuffer<float>>("Features").First();

        // Convert VBuffers to arrays and compute cosine similarity
        float[] q = queryFeatures.DenseValues().ToArray();
        double qNorm = Math.Sqrt(q.Select(v => (double)v * v).Sum());
        if (qNorm == 0)
        {
            qNorm = 1; // avoid division by zero
        }

        var scored = new List<(DataSourceEntity ds, double score)>();
        for (int i = 0; i < dataSources.Count; i++)
        {
            float[] v = featuresColumn[i].DenseValues().ToArray();
            double dot = 0;
            for (int j = 0; j < Math.Min(q.Length, v.Length); j++)
            {
                dot += q[j] * v[j];
            }

            double vNorm = Math.Sqrt(v.Select(x => (double)x * x).Sum());
            if (vNorm == 0)
            {
                vNorm = 1;
            }

            double cosine = dot / (qNorm * vNorm);
            scored.Add((dataSources[i], cosine));
        }

        // Order by descending relevance and take up to max
        IEnumerable<DataSourceEntity> ordered = scored
            .OrderByDescending(t => t.score)
            .Take(max)
            .Select(t => t.ds);

        // Build response with access flags
        List<DataSourceRecommendationBindingModel> response = ordered.Select(d =>
        {
            List<RequiredSkillAccessBindingModel> reqSkills = d.Requirements
                .Select(r => r.Skill)
                .DistinctBy(s => s.Id)
                .Select(s => new RequiredSkillAccessBindingModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    HasSkill = userSkills.Contains(s.Id)
                })
                .ToList();

            bool hasAccess = reqSkills.All(rs => rs.HasSkill);

            return new DataSourceRecommendationBindingModel
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                RequiredSkills = reqSkills,
                HasAccess = hasAccess
            };
        }).ToList();

        await this.Send.OkAsync(response, ct);
    }

    private static string Combine(string name, string? description)
        => string.IsNullOrWhiteSpace(description) ? name : $"{name} {description}";

    private sealed class TextDoc
    {
        public string Text { get; set; } = string.Empty;
    }
}
