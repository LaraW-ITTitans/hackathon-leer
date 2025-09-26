using System.ComponentModel.DataAnnotations;
using ITTitans.Hackathon2025.Utils;

namespace ITTitans.Hackathon2025.WebAPI.Model.User;

public class CreateUserBindingModel
{
    [StringLength(StringLengths.UserName)]
    public required string UserName { get; init; }

    [StringLength(StringLengths.Name)]
    public required string DisplayName { get; init; }

    [StringLength(StringLengths.Description)]
    public required string Password { get; init; }
}