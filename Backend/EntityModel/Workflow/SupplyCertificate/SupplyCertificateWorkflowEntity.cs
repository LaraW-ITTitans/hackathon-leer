using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;

[Table(name: "SupplyCertificateWorkflow")]
public class SupplyCertificateWorkflowEntity
{
    public Guid Id { get; set; }

    public SupplyCertificateWorkflowStateType State { get; set; }

    public HackathonUserEntity Initiator { get; set; } = null!;
    
    public Guid InitiatorId { get; set; }

    public HackathonUserEntity? Reviewer { get; set; }
    
    public Guid? ReviewerId { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Completed { get; set; }
}