using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ITTitans.Hackathon2025.EntityModel.Attachment;

[Table(name: "Attachment")]
public class AttachmentEntity
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(StringLengths.FileNameWithoutExtension)]
    public string FileNameWithoutExtension { get; set; } = null!;

    [StringLength(10)]
    public string FileExtension { get; set; } = null!;
    
    [StringLength(StringLengths.ContentType)]
    public string ContentType { get; set; } = null!;
    
    public long FileSizeInBytes { get; set; }
    
    public HackathonUserEntity CreatedBy { get; set; } = null!;
    
    public Guid CreatedById { get; set; }
    
    public DateTimeOffset Created { get; set; }
    
    public IList<AttachmentLinkEntity> Links { get; } = [];

    public int Version { get; set; }
    
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "EF Core API")]
    public required byte[] Content { get; set; }
}