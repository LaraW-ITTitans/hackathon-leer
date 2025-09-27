namespace ITTitans.Hackathon2025.WebAPI.Model.Skill;

public class RequiredSkillAccessBindingModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public bool HasSkill { get; init; }
}
