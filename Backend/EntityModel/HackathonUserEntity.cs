using ITTitans.Hackathon2025.Utils;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.EntityModel;

public class HackathonUserEntity : IdentityUser<Guid>
{
    [StringLength(StringLengths.Name)]
    public string DisplayName { get; set; } = null!;

    public bool IsDeleted { get; set; }
}
