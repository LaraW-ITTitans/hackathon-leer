using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;

namespace ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;

public class BasicSupplyCertificateWorkflowBindingModel
{
    public Guid Id { get; set; }
    
    public SupplyCertificateWorkflowStateType State { get; set; }
}