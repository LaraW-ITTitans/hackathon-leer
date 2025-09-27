using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Attachment;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class GetSupplyCertificateWorkflowFileEndpoint : EndpointWithoutRequest<SupplyCertificateWorkflowFileBindingModel>
{
    private readonly HackathonDbContext dbContext;

    public GetSupplyCertificateWorkflowFileEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/workflows/supply-certificates/{id:guid}/file");
        this.AddHackathonPolicy(AuthClaimType.SupplyCertificateWorkflowSee);
        this.Description(b => b
            .WithName("GetSupplyCertificateWorkflowFile")
            .WithTags("SupplyCertificateWorkflows"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var workflowId = this.Route<Guid>("id");

        // find the attachment linked to this workflow
        var link = await this.dbContext.AttachmentLinks
            .Include(l => l.Attachment)
            .Where(l => l.EntityType == EntityType.SupplyCertificateWorkflow && l.EntityKey == workflowId)
            .OrderByDescending(l => l.Attachment.Version)
            .FirstOrDefaultAsync(ct);

        if (link is null || link.Attachment is null)
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 404;
            return;
        }

        var attachment = link.Attachment;
        var fileName = string.IsNullOrWhiteSpace(attachment.FileExtension)
            ? attachment.FileNameWithoutExtension
            : $"{attachment.FileNameWithoutExtension}.{attachment.FileExtension}";

        var response = new SupplyCertificateWorkflowFileBindingModel
        {
            FileName = fileName,
            ContentBase64 = Convert.ToBase64String(attachment.Content)
        };

        await this.Send.OkAsync(response, ct);
    }
}
