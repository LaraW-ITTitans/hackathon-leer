using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.DataSource;

public class DeleteDataSourceEndpoint : EndpointWithoutRequest
{
    private readonly HackathonDbContext dbContext;
    private readonly ILogger<DeleteDataSourceEndpoint> logger;

    public DeleteDataSourceEndpoint(HackathonDbContext dbContext, ILogger<DeleteDataSourceEndpoint> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public override void Configure()
    {
        this.Delete("api/data-sources/{id:guid}");
        this.AddHackathonPolicy(AuthClaimType.ManageDataSource);

        this.Description(b => b
            .WithName("DeleteDataSource")
            .WithTags("DataSources"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = this.Route<Guid>("id");

        var entity = await this.dbContext.DataSources
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);

        if (entity is null)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        entity.IsDeleted = true;
        await this.dbContext.SaveChangesAsync(ct);

        this.logger.LogInformation("Deleted data source {Id}", id);

        await this.Send.OkAsync(cancellation: ct);
    }
}