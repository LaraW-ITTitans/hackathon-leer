namespace ITTitans.Hackathon2025.WebAPI.Model.Skill;

public class SkillWithCertificateBindingModel
{
    public required Guid SkillId { get; init; }
    public required string SkillName { get; init; }
    public required Guid WorkflowId { get; init; }
    public required string CertificateFileName { get; init; }
    public required string CertificateContentType { get; init; }
    public required long CertificateSizeInBytes { get; init; }
    public required string CertificateContentBase64 { get; init; }
}