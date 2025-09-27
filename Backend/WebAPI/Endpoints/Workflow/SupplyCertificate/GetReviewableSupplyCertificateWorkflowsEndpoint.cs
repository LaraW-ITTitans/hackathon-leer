using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.Utils;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class GetReviewableSupplyCertificateWorkflowsEndpoint : EndpointWithoutRequest<IEnumerable<BasicSupplyCertificateWorkflowBindingModel>>
{
    private readonly HackathonDbContext dbContext;

    public GetReviewableSupplyCertificateWorkflowsEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Get("api/workflows/supply-certificates/reviewable");
        this.AddHackathonPolicy(AuthClaimType.SupplyCertificateWorkflowProcess);
        this.Description(b => b
            .WithName("GetReviewableSupplyCertificateWorkflows")
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
        
        HashSet<Guid> allowedSkillClaims;
        
        bool isAdmin = StaticData.PredefinedAdminUserId == userId;
        if (isAdmin)
        {
            allowedSkillClaims = await this.dbContext.Skills
                .Where(x => !x.IsDeleted)
                .Select(x => x.Id)
                .ToHashSetAsync(ct);
        }
        else
        {
            allowedSkillClaims = this.User.FindAll(HackathonClaims.PossibleSkillsToReviewClaimName)
                .Select(c => c.Value)
                .Where(v => Guid.TryParse(v, out _))
                .Select(Guid.Parse)
                .ToHashSet();
        }

        List<BasicSupplyCertificateWorkflowBindingModel> items = await this.dbContext.SupplyCertificateWorkflows
            .AsNoTracking()
            .Where(w => w.State == SupplyCertificateWorkflowStateType.InReview && allowedSkillClaims.Contains(w.SkillId))
            .Select(w => new BasicSupplyCertificateWorkflowBindingModel
            {
                Id = w.Id,
                State = w.State,
                Initiator = new BasicUserBindingModel
                {
                    Id = w.Initiator.Id,
                    UserName = w.Initiator.UserName!,
                    DisplayName = w.Initiator.DisplayName
                },
                Skill = new BasicSkillBindingModel
                {
                    Id = w.Skill.Id,
                    Name = w.Skill.Name
                }
            })
            .ToListAsync(ct);

        await this.Send.OkAsync(items, ct);
    }
}
