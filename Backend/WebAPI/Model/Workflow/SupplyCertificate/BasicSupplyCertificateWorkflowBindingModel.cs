using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Model.User;

namespace ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;

public class BasicSupplyCertificateWorkflowBindingModel
{
    public Guid Id { get; set; }
    
    public SupplyCertificateWorkflowStateType State { get; set; }

    public required BasicUserBindingModel Initiator { get; init; }

    public required BasicSkillBindingModel Skill { get; init; }
}