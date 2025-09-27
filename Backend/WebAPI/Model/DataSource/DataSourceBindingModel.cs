using ITTitans.Hackathon2025.WebAPI.Model.Skill;

namespace ITTitans.Hackathon2025.WebAPI.Model.DataSource;

public class DataSourceBindingModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<BasicSkillBindingModel> RequiredSkills { get; init; } = Array.Empty<BasicSkillBindingModel>();
}