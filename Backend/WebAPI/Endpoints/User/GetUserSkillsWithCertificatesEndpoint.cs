using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Attachment;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.User;

public class GetUserSkillsWithCertificatesEndpoint : EndpointWithoutRequest<IEnumerable<SkillWithCertificateBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetUserSkillsWithCertificatesEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/users/{id:guid}/skills-with-certificates");
        // No global policy: authorization is enforced in handler (self or SeeUser)
        this.Description(b => b
            .WithName("GetUserSkillsWithCertificates")
            .WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var targetUserId = this.Route<Guid>("id");

        string? currentUserIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        bool isAuthenticated = Guid.TryParse(currentUserIdClaim, out Guid currentUserId);

        bool isSelf = isAuthenticated && currentUserId == targetUserId;
        bool hasSeeUser = this.User.HasClaim(c => c is { Type: HackathonClaims.AuthClaimName, Value: nameof(AuthClaimType.SeeUser) });

        if (!isSelf && !hasSeeUser)
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 403;
            return;
        }

        List<SkillAssignmentEntity> assignments = await this.dbContext.SkillAssignments
            .AsNoTracking()
            .Where(a => a.UserId == targetUserId)
            .Include(a => a.Skill)
            .ToListAsync(ct);

        if (assignments.Count == 0)
        {
            await this.Send.OkAsync([], ct);
            return;
        }

        Guid[] workflowIds = assignments.Select(a => a.WorkflowId).Distinct().ToArray();

        // Fetch attachments linked to workflows
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
