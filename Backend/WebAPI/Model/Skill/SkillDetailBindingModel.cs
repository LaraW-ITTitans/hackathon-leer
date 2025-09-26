using ITTitans.Hackathon2025.WebAPI.Model.User;

namespace ITTitans.Hackathon2025.WebAPI.Model.Skill;

public class SkillDetailBindingModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required IEnumerable<BasicUserBindingModel> PossibleReviewers { get; init; }
}
