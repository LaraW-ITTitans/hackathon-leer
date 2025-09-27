using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.Skill;

[Table(name: "SkillAssignment")]
public class SkillAssignmentEntity
{
    public Guid Id { get; set; }

    public HackathonUserEntity User { get; set; } = null!;

    public Guid UserId { get; set; }

    public SkillEntity Skill { get; set; } = null!;

    public Guid SkillId { get; set; }

    public SupplyCertificateWorkflowEntity Workflow { get; set; } = null!;

    public Guid WorkflowId { get; set; }
}