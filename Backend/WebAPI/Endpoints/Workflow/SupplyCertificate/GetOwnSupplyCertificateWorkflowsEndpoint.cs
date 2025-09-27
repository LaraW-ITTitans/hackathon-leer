using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class GetOwnSupplyCertificateWorkflowsEndpoint : EndpointWithoutRequest<IEnumerable<BasicSupplyCertificateWorkflowBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetOwnSupplyCertificateWorkflowsEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/workflows/supply-certificates/own");
        this.Description(b => b
            .WithName("GetOwnSupplyCertificateWorkflows")
            .WithTags("SupplyCertificateWorkflows"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }

        List<BasicSupplyCertificateWorkflowBindingModel> items = await this.dbContext.SupplyCertificateWorkflows
            .AsNoTracking()
            .Where(w => w.InitiatorId == userId)
            .Select(w => new BasicSupplyCertificateWorkflowBindingModel
            {
                Id = w.Id,
                State = w.State,
            })
            .ToListAsync(ct);

        await this.Send.OkAsync(items, ct);
    }
}
