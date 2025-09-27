using System.Security.Claims;
using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Model.Workflow.SupplyCertificate;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Workflow.SupplyCertificate;

public class ProcessSupplyCertificateWorkflowEndpoint : Endpoint<ProcessSupplyCertificateWorkflowBindingModel, BasicSupplyCertificateWorkflowBindingModel>
{
    private readonly HackathonDbContext dbContext;

    public ProcessSupplyCertificateWorkflowEndpoint(HackathonDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public override void Configure()
    {
        this.Post("api/workflows/supply-certificates/{id:guid}/process");
        this.AddHackathonPolicy(AuthClaimType.SupplyCertificateWorkflowProcess);
        
        this.Description(b => b
            .WithName("ProcessSupplyCertificateWorkflow")
            .WithTags("SupplyCertificateWorkflows"));
    }

    public override async Task HandleAsync(ProcessSupplyCertificateWorkflowBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        
        var workflowId = this.Route<Guid>("id");

        if (string.IsNullOrWhiteSpace(req.ReviewComment))
        {
            this.AddError("ReviewComment is required.");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        string? userIdClaim = this.User.FindFirstValue(HackathonClaims.UserIdClaimName);
        if (!Guid.TryParse(userIdClaim, out Guid reviewerId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 401;
            return;
        }
        
        SupplyCertificateWorkflowEntity? workflow = await this.dbContext.SupplyCertificateWorkflows
            .Include(w => w.Skill)
            .FirstOrDefaultAsync(w => w.Id == workflowId, ct);
        if (workflow is null)
        {
            this.AddError($"workflow with id {workflowId} not found");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (workflow.State != SupplyCertificateWorkflowStateType.InReview)
        {
            this.AddError("workflow is not in review state");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }
        
        HashSet<Guid> allowedSkillClaims = this.User.FindAll(HackathonClaims.PossibleSkillsToReviewClaimName)
            .Select(c => c.Value)
            .Where(v => Guid.TryParse(v, out _))
            .Select(Guid.Parse)
            .ToHashSet();

        if (!allowedSkillClaims.Contains(workflow.SkillId))
        {
            await this.HttpContext.Response.StartAsync(ct);
            this.HttpContext.Response.StatusCode = 403;
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        workflow.ReviewerId = reviewerId;
        workflow.ReviewComment = req.ReviewComment;
        workflow.Completed = now;
        workflow.State = req.Accept ? SupplyCertificateWorkflowStateType.Accepted : SupplyCertificateWorkflowStateType.Declined;

        if (req.Accept)
        {
            bool assignmentExists = await this.dbContext.SkillAssignments
                .AnyAsync(a => a.UserId == workflow.InitiatorId && a.SkillId == workflow.SkillId, ct);

            if (!assignmentExists)
            {
                var assignment = new SkillAssignmentEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = workflow.InitiatorId,
                    SkillId = workflow.SkillId,
                    WorkflowId = workflow.Id,
                };

                await this.dbContext.SkillAssignments.AddAsync(assignment, ct);
            }
        }

        await this.dbContext.SaveChangesAsync(ct);

        var initiatorBasic = await this.dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == workflow.InitiatorId)
            .Select(u => new BasicUserBindingModel
            {
                Id = u.Id,
                UserName = u.UserName!,
                DisplayName = u.DisplayName
            })
            .FirstAsync(ct);

        var skillBasic = await this.dbContext.Skills
            .AsNoTracking()
            .Where(s => s.Id == workflow.SkillId)
            .Select(s => new BasicSkillBindingModel
            {
                Id = s.Id,
                Name = s.Name
            })
            .FirstAsync(ct);

        await this.Send.OkAsync(new BasicSupplyCertificateWorkflowBindingModel
        {
            Id = workflow.Id,
            State = workflow.State,
            Initiator = initiatorBasic,
            Skill = skillBasic
        }, ct);
    }
}