using ITTitans.Hackathon2025.WebAPI.Model.Skill;

namespace ITTitans.Hackathon2025.WebAPI.Model.User;

public class DetailedUserBindingModel
{
    public required Guid Id { get; init; }
    
    public required string UserName { get; init; }
    
    public required string DisplayName { get; init; }

    public required IEnumerable<BasicSkillBindingModel> Skills { get; init; }

    public required IEnumerable<BasicSkillBindingModel> ReviewableSkills { get; init; }
}