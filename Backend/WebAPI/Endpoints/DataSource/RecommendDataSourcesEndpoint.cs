using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.DataSource;
using ITTitans.Hackathon2025.Service.Interfaces.Settings;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.DataSource;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

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

        if (!this.TryGetAuthenticatedUserId(out Guid userId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }

        HashSet<Guid> userSkills = await this.LoadUserSkillsAsync(userId, ct);
        List<DataSourceEntity> dataSources = await this.LoadDataSourcesAsync(ct);

        if (dataSources.Count == 0)
        {
            await this.Send.OkAsync([], ct);
            return;
        }
        
        List<Guid> orderedIds = await this.GetRecommendationsFromOpenAiAsync(req.Query, dataSources, max, ct);
        IEnumerable<DataSourceRecommendationBindingModel> response = BuildResponse(orderedIds, dataSources, userSkills);

        await this.Send.OkAsync(response, ct);
    }

    // --- Data access
    private async Task<HashSet<Guid>> LoadUserSkillsAsync(Guid userId, CancellationToken ct)
    {
        return (await this.hackathonDbContext.SkillAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.SkillId)
            .ToListAsync(ct)).ToHashSet();
    }

    private async Task<List<DataSourceEntity>> LoadDataSourcesAsync(CancellationToken ct)
    {
        return await this.hackathonDbContext.DataSources
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.Requirements)
            .ThenInclude(r => r.Skill)
            .ToListAsync(ct);
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        return Guid.TryParse(userIdClaim, out userId);
    }
    
    private async Task<List<Guid>> GetRecommendationsFromOpenAiAsync(
        string query,
        List<DataSourceEntity> dataSources,
        int max,
        CancellationToken cancellationToken)
    {
        string googleAiApiKey = this.webServerAppSettingsService.GetGoogleAiApiKey();
        
        if (string.IsNullOrWhiteSpace(googleAiApiKey))
        {
            return FallbackKeywordRanking(query, dataSources, max);
        }

        try
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(20);
            
            var items = dataSources.Select(ds => new
            {
                id = ds.Id,
                text = Combine(ds.Name, ds.Description)
            }).ToArray();

            // Build a single prompt instructing Gemini to return strict JSON
            string prompt = new StringBuilder()
                .AppendLine("You are a ranking assistant. Given a search query and a list of data sources (id and text), return the top N ids sorted by relevance to the query. Output strictly JSON with property 'ids' as an array of GUID strings. No extra text.")
                .AppendLine($"Query: {query}")
                .AppendLine($"Max: {max}")
                .AppendLine("DataSources:")
                .AppendLine(string.Join('\n', items.Select(i => $"- {{\"id\":\"{i.id}\",\"text\":\"{JsonEscape(i.text)}\"}}")))
                .AppendLine("Return JSON: {\"ids\":[\"...\"]}")
                .ToString();

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[] { new { text = prompt } }
                    }
                },
                generationConfig = new { temperature = 0 }
            };

            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            string url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={googleAiApiKey}";
            using HttpResponseMessage resp = await http.PostAsync(url, content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                string errorContent = await resp.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine(errorContent);
                return FallbackKeywordRanking(query, dataSources, max);
            }

            await using Stream stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            JsonElement root = doc.RootElement;
            string? json = null;
            if (root.TryGetProperty("candidates", out JsonElement cands) &&
                cands.ValueKind == JsonValueKind.Array && cands.GetArrayLength() > 0)
            {
                JsonElement cand0 = cands[0];
                if (cand0.TryGetProperty("content", out JsonElement contentObj) &&
                    contentObj.TryGetProperty("parts", out JsonElement partsEl) &&
                    partsEl.ValueKind == JsonValueKind.Array && partsEl.GetArrayLength() > 0)
                {
                    foreach (JsonElement part in partsEl.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out JsonElement textEl) && textEl.ValueKind == JsonValueKind.String)
                        {
                            json = textEl.GetString();
                            if (!string.IsNullOrWhiteSpace(json)) break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return FallbackKeywordRanking(query, dataSources, max);
            }

            using JsonDocument idsDoc = JsonDocument.Parse(json);
            if (!idsDoc.RootElement.TryGetProperty("ids", out JsonElement idsEl) || idsEl.ValueKind != JsonValueKind.Array)
            {
                return FallbackKeywordRanking(query, dataSources, max);
            }

            var ids = new List<Guid>();
            foreach (JsonElement idEl in idsEl.EnumerateArray())
            {
                if (idEl.ValueKind == JsonValueKind.String && Guid.TryParse(idEl.GetString(), out Guid g))
                {
                    ids.Add(g);
                    if (ids.Count >= max)
                    {
                        break;
                    }
                }
            }

            // Ensure uniqueness and limit to existing IDs
            HashSet<Guid> set = dataSources.Select(d => d.Id).ToHashSet();
            List<Guid> filtered = ids.Where(set.Contains).Distinct().Take(max).ToList();

            // If the model returned less than requested, append fallback-ranked remainder
            if (filtered.Count < Math.Min(max, dataSources.Count))
            {
                List<Guid> fallback = FallbackKeywordRanking(query, dataSources.Where(d => !filtered.Contains(d.Id)).ToList(), max - filtered.Count);
                filtered.AddRange(fallback);
            }

            return filtered;
        }
        catch
        {
            return FallbackKeywordRanking(query, dataSources, max);
        }
    }

    private static List<Guid> FallbackKeywordRanking(string query, List<DataSourceEntity> dataSources, int max)
    {
        if (dataSources.Count == 0 || max <= 0)
        {
            return new List<Guid>();
        }

        string[] terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant()).ToArray();

        return dataSources
            .Select(ds => new
            {
                ds.Id,
                Score = Score(Combine(ds.Name, ds.Description), terms)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Id)
            .Take(Math.Min(max, dataSources.Count))
            .Select(x => x.Id)
            .ToList();

        static int Score(string text, string[] terms)
        {
            string lower = text.ToLowerInvariant();
            int s = 0;
            foreach (string t in terms)
            {
                if (string.IsNullOrWhiteSpace(t))
                {
                    continue;
                }

                if (lower.Contains(t))
                {
                    s += 1;
                }
            }
            return s;
        }
    }

    // --- Build response
    private static IEnumerable<DataSourceRecommendationBindingModel> BuildResponse(List<Guid> orderedIds, List<DataSourceEntity> dataSources, HashSet<Guid> userSkills)
    {
        Dictionary<Guid, DataSourceEntity> byId = dataSources.ToDictionary(d => d.Id);
        var result = new List<DataSourceRecommendationBindingModel>(orderedIds.Count);
        foreach (Guid id in orderedIds)
        {
            if (!byId.TryGetValue(id, out DataSourceEntity? ds))
            {
                continue;
            }

            List<RequiredSkillAccessBindingModel> required = ds.Requirements
                .Select(r => new RequiredSkillAccessBindingModel
                {
                    Id = r.SkillId,
                    Name = r.Skill.Name,
                    HasSkill = userSkills.Contains(r.SkillId)
                })
                .ToList();

            bool hasAccess = required.All(s => s.HasSkill);

            result.Add(new DataSourceRecommendationBindingModel
            {
                Id = ds.Id,
                Name = ds.Name,
                Description = ds.Description,
                HasAccess = hasAccess,
                RequiredSkills = required
            });
        }
        return result;
    }

    private static string Combine(string name, string? description)
        => string.IsNullOrWhiteSpace(description) ? name : $"{name} {description}";

    private static string JsonEscape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
}
