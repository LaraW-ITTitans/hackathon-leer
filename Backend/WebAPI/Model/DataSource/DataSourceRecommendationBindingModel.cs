using ITTitans.Hackathon2025.WebAPI.Model.Skill;

namespace ITTitans.Hackathon2025.WebAPI.Model.DataSource;

public class DataSourceRecommendationBindingModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }

    // Whether the current user has all required skills for this data source
    public bool HasAccess { get; init; }

    // Required skills with per-skill access information
    public IReadOnlyList<RequiredSkillAccessBindingModel> RequiredSkills { get; init; } = Array.Empty<RequiredSkillAccessBindingModel>();
}
