using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Attachment;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class StartSupplyCertificateWorkflowEndpoint : Endpoint<StartSupplyCertificateWorkflowBindingModel, BasicSupplyCertificateWorkflowBindingModel>
{
    private readonly HackathonDbContext dbContext;

    public StartSupplyCertificateWorkflowEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Post("api/workflows/supply-certificates");
        this.AddHackathonPolicy(AuthClaimType.SupplyCertificateWorkflowStart);
        this.AllowFormData();

        this.Description(b => b
            .WithName("StartSupplyCertificateWorkflow")
            .WithTags("SupplyCertificateWorkflows"));
    }

    public override async Task HandleAsync(StartSupplyCertificateWorkflowBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        
        if (req.File == null! || req.File.Length == 0)
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 400;
            await this.HttpContext.Response.BodyWriter.WriteAsync("File is required."u8.ToArray(), ct);
            return;
        }

        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }

        // read file content
        byte[] content;
        using (var ms = new MemoryStream())
        {
            await req.File.CopyToAsync(ms, ct);
            content = ms.ToArray();
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        SkillEntity? skill = await this.dbContext.Skills
            .FirstOrDefaultAsync(x => x.Id == req.SkillId && !x.IsDeleted, ct);
        if (skill is null)
        {
            this.AddError($"skill with id {req.SkillId} not found");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }
        
        // create workflow
        var workflow = new SupplyCertificateWorkflowEntity
        {
            Id = Guid.NewGuid(),
            State = SupplyCertificateWorkflowStateType.InReview,
            InitiatorId = userId,
            Skill = skill,
            Created = now,
        };

        // create attachment
        string fileName = req.File.FileName;
        string fileExt = Path.GetExtension(fileName);
        if (fileExt.StartsWith('.'))
        {
            fileExt = fileExt[1..];
        }

        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

        var attachment = new AttachmentEntity
        {
            Id = Guid.NewGuid(),
            FileNameWithoutExtension = nameWithoutExt,
            FileExtension = fileExt,
            ContentType = req.File.ContentType,
            FileSizeInBytes = req.File.Length,
            CreatedById = userId,
            Created = now,
            Version = 1,
            Content = content,
        };

        await this.dbContext.SupplyCertificateWorkflows.AddAsync(workflow, ct);
        await this.dbContext.Attachments.AddAsync(attachment, ct);
        await this.dbContext.SaveChangesAsync(ct);

        var link = new AttachmentLinkEntity
        {
            Id = Guid.NewGuid(),
            AttachmentId = attachment.Id,
            EntityType = EntityType.SupplyCertificateWorkflow,
            EntityKey = workflow.Id,
        };

        await this.dbContext.AttachmentLinks.AddAsync(link, ct);
        await this.dbContext.SaveChangesAsync(ct);

        await this.Send.OkAsync(new BasicSupplyCertificateWorkflowBindingModel
        {
            Id = workflow.Id,
            State = workflow.State,
        }, ct);
    }
}