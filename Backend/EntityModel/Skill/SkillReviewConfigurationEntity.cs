using ITTitans.Hackathon2025.EntityModel.Auth;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.Skill;

[Table(name: "SkillReviewConfiguration")]
public class SkillReviewConfigurationEntity
{
    public Guid Id { get; set; }

    public SkillEntity Skill { get; set; } = null!;

    public Guid SkillId { get; set; }

    public HackathonUserEntity User { get; set; } = null!;

    public Guid UserId { get; set; }
}