using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;

public class ProcessSupplyCertificateWorkflowBindingModel
{
    [Required]
    public bool Accept { get; set; }

    [Required]
    [MinLength(1)]
    public string ReviewComment { get; set; } = string.Empty;
}