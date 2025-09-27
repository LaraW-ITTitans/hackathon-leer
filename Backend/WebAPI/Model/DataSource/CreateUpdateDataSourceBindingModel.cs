using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.DataSource;

public class CreateUpdateDataSourceBindingModel
{
    [Required]
    public required string Name { get; init; }

    public string? Description { get; init; }

    public Guid[] RequiredSkillIds { get; init; } = [];
}