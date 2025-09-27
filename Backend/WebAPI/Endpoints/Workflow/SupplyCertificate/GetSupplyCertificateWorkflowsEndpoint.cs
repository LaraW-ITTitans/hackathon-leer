using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class GetSupplyCertificateWorkflowsEndpoint : EndpointWithoutRequest<IEnumerable<BasicSupplyCertificateWorkflowBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetSupplyCertificateWorkflowsEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/workflows/supply-certificates");
        this.AddHackathonPolicy(AuthClaimType.SupplyCertificateWorkflowSee);
        this.Description(b => b
            .WithName("GetSupplyCertificateWorkflows")
            .WithTags("SupplyCertificateWorkflows"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<BasicSupplyCertificateWorkflowBindingModel> items = await this.dbContext.SupplyCertificateWorkflows
            .AsNoTracking()
            .Select(w => new BasicSupplyCertificateWorkflowBindingModel
            {
                Id = w.Id,
                State = w.State,
            })
            .ToListAsync(ct);

        await this.Send.OkAsync(items, ct);
    }
}
