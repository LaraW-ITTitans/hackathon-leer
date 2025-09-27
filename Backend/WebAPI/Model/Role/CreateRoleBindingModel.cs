using ITTitans.Hackathon2025.Utils;
using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.Role;

public class CreateRoleBindingModel
{
    [Required]
    [StringLength(StringLengths.Name)]
    public required string Name { get; init; }
    
    [StringLength(StringLengths.Description)]
    public string? Description { get; set; }
}