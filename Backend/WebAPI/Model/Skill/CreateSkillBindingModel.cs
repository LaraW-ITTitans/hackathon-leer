using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.Skill;

public class CreateSkillBindingModel
{
    [Required]
    public required string Name { get; init; }
    public string? Description { get; init; }
}
