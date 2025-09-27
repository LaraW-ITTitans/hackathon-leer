using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Attachment;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class GetCurrentUserSkillsWithCertificatesEndpoint : EndpointWithoutRequest<IEnumerable<SkillWithCertificateBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetCurrentUserSkillsWithCertificatesEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/users/me/skills-with-certificates");
        this.Description(b => b
            .WithName("GetCurrentUserSkillsWithCertificates")
            .WithTags("Users"));
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

        List<SkillAssignmentEntity> assignments = await this.dbContext.SkillAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Include(a => a.Skill)
            .ToListAsync(ct);

        if (assignments.Count == 0)
        {
            await this.Send.OkAsync([], ct);
            return;
        }

        Guid[] workflowIds = assignments.Select(a => a.WorkflowId).Distinct().ToArray();

        List<AttachmentLinkEntity> links = await this.dbContext.AttachmentLinks
            .AsNoTracking()
            .Where(l => l.EntityType == EntityType.SupplyCertificateWorkflow && workflowIds.Contains(l.EntityKey))
            .Include(l => l.Attachment)
            .ToListAsync(ct);

        Dictionary<Guid, AttachmentEntity> linkMap = links
            .GroupBy(l => l.EntityKey)
            .ToDictionary(g => g.Key, g => g
                .OrderByDescending(l => l.Attachment.Version)
                .First().Attachment);

        List<SkillWithCertificateBindingModel> result = assignments
            .Select(a => new { a.SkillId, a.Skill.Name, a.WorkflowId, Attachment = linkMap.GetValueOrDefault(a.WorkflowId) })
            .Where(x => x.Attachment != null)
            .Select(x => new SkillWithCertificateBindingModel
            {
                SkillId = x.SkillId,
                SkillName = x.Name,
                WorkflowId = x.WorkflowId,
                CertificateFileName = $"{x.Attachment!.FileNameWithoutExtension}.{x.Attachment.FileExtension}",
                CertificateContentType = x.Attachment.ContentType,
                CertificateSizeInBytes = x.Attachment.FileSizeInBytes,
                CertificateContentBase64 = Convert.ToBase64String(x.Attachment.Content)
            })
            .ToList();

        await this.Send.OkAsync(result, ct);
    }
}
