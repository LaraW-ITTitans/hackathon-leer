using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class CancelSupplyCertificateWorkflowEndpoint : EndpointWithoutRequest<BasicSupplyCertificateWorkflowBindingModel>
{
    private readonly HackathonDbContext dbContext;

    public CancelSupplyCertificateWorkflowEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Post("api/workflows/supply-certificates/{id:guid}/cancel");
        
        this.Description(b => b
            .WithName("CancelSupplyCertificateWorkflow")
            .WithTags("SupplyCertificateWorkflows"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var workflowId = this.Route<Guid>("id");

        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }

        SupplyCertificateWorkflowEntity? workflow = await this.dbContext.SupplyCertificateWorkflows
            .FirstOrDefaultAsync(w => w.Id == workflowId, ct);
        if (workflow is null)
        {
            this.AddError($"workflow with id {workflowId} not found");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (workflow.InitiatorId != userId)
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 403;
            return;
        }

        if (workflow.State != SupplyCertificateWorkflowStateType.InReview)
        {
            this.AddError("workflow is not in a cancellable state");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        workflow.State = SupplyCertificateWorkflowStateType.Cancelled;
        workflow.Completed = DateTimeOffset.UtcNow;

        await this.dbContext.SaveChangesAsync(ct);

        await this.Send.OkAsync(new BasicSupplyCertificateWorkflowBindingModel
        {
            Id = workflow.Id,
            State = workflow.State,
        }, ct);
    }
}
