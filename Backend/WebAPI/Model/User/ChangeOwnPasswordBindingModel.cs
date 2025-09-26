using System.ComponentModel.DataAnnotations;
using ITTitans.Hackathon2025.Utils;

namespace ITTitans.Hackathon2025.WebAPI.Model.User;

public class ChangeOwnPasswordBindingModel
{
    [StringLength(StringLengths.Description)]
    public required string CurrentPassword { get; init; }

    [StringLength(StringLengths.Description)]
    public required string NewPassword { get; init; }
}