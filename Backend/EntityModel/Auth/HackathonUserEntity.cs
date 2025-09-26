using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.Utils;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.EntityModel.Auth;

public class HackathonUserEntity : IdentityUser<Guid>
{
    [StringLength(StringLengths.Name)]
    public string DisplayName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public IList<SkillEntity> PossibleSkillsToReview { get; init; } = [];

    public IList<SkillAssignmentEntity> SkillAssignments { get; init; } = [];

    public IList<SupplyCertificateWorkflowEntity> InitiatedSupplyCertificateWorkflows { get; init; } = [];

    public IList<SupplyCertificateWorkflowEntity> ReviewedSupplyCertificateWorkflows { get; init; } = [];
}
