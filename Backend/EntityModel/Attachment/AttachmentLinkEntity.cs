using ITTitans.Hackathon2025.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTitans.Hackathon2025.EntityModel.Attachment;

[Table(name: "AttachmentLink")]
public class AttachmentLinkEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public AttachmentEntity Attachment { get; set; } = null!;
    
    public Guid AttachmentId { get; set; }
    
    public required EntityType EntityType { get; set; }
    
    public Guid EntityKey { get; set; }
}