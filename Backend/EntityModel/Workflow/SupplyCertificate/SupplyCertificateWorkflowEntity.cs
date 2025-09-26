using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;

[Table(name: "SupplyCertificateWorkflow")]
public class SupplyCertificateWorkflowEntity
{
    public Guid Id { get; set; }

    public SupplyCertificateWorkflowStateType State { get; set; }

    public HackathonUserEntity Initiator { get; set; } = null!;
    
    public Guid InitiatorId { get; set; }

    public SkillEntity Skill { get; set; } = null!;

    public Guid SkillId { get; set; }

    public HackathonUserEntity? Reviewer { get; set; }
    
    public Guid? ReviewerId { get; set; }

    [StringLength(StringLengths.Comment)]
    public string? ReviewComment { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Completed { get; set; }
}