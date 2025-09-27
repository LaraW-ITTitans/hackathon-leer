using ITTitans.Hackathon2025.EntityModel.Skill;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.DataSource;

[Table(name: "DataSourceRequirement")]
public class DataSourceRequirementEntity
{
    public Guid Id { get; set; }

    public DataSourceEntity DataSource { get; set; } = null!;

    public Guid DataSourceId { get; set; }

    public SkillEntity Skill { get; set; } = null!;

    public Guid SkillId { get; set; }
}