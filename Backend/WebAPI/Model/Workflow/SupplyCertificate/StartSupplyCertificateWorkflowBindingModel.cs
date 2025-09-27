namespace ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;

public class StartSupplyCertificateWorkflowBindingModel
{
    public Guid SkillId { get; set; }
    
    public IFormFile File { get; set; } = null!;
}