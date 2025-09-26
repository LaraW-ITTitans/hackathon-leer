namespace ITTitans.Hackathon2025.Service.Interfaces;

public interface IEnsureCreatedPredefinedEntitiesService
{
    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
}