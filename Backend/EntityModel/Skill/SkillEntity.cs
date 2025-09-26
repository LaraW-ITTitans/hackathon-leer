using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.Skill;

[Table(name: "Skill")]
public class SkillEntity
{
    public Guid Id { get; set; }

    [StringLength(StringLengths.Name)]
    public string Name { get; set; } = null!;
    
    [StringLength(StringLengths.Description)]
    public string? Description { get; set; }

    public IList<HackathonUserEntity> PossibleReviewer { get; init; } = [];
}