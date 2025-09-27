namespace ITTitans.Hackathon2025.WebAPI.Model.Role;

public class BasicRoleBindingModel
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required string? Description { get; init; }
}