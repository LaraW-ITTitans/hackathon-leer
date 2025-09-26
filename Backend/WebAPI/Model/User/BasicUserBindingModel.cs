namespace ITTitans.Hackathon2025.WebAPI.Model.User;

public class BasicUserBindingModel
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string DisplayName { get; init; }
}