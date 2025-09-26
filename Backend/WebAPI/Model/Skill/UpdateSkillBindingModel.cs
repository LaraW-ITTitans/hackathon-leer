using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.Skill;

public class UpdateSkillBindingModel
{
    [Required]
    public required Guid Id { get; init; }
    [Required]
    public required string Name { get; init; }
    public string? Description { get; init; }
}
