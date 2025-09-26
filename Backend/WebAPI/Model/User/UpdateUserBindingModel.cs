using System.ComponentModel.DataAnnotations;
using ITTitans.Hackathon2025.Utils;

namespace ITTitans.Hackathon2025.WebAPI.Model.User;

public class UpdateUserBindingModel
{
    public required Guid Id { get; init; }

    [StringLength(StringLengths.UserName)]
    public required string UserName { get; init; }

    [StringLength(StringLengths.Name)]
    public required string DisplayName { get; init; }

    // Optional; if null or empty, do not change password
    [StringLength(StringLengths.Description)]
    public string? Password { get; init; }
}