using ITTitans.Hackathon2025.Utils;
using System.ComponentModel.DataAnnotations;

namespace ITTitans.Hackathon2025.WebAPI.Model.Role;

public class UpdateRoleBindingModel
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    [StringLength(StringLengths.Name)]
    public required string Name { get; init; }
}
