using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.Skill;

public class UpdateSkillReviewersBindingModel
{
    [Required]
    public required Guid SkillId { get; init; }

    [Required]
    public required IEnumerable<Guid> ReviewerUserIds { get; init; }
}
