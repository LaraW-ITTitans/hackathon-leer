using ITTitans.Hackathon2025.Utils;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.EntityModel.Auth;

public class HackathonRoleEntity : IdentityRole<Guid>
{
    [StringLength(StringLengths.Description)]
    public string? Description { get; set; }
    
    public bool IsDeleted { get; set; }
}
