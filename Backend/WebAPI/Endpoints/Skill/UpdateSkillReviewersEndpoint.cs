using FastEndpoints;
using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.WebAPI.Model.Skill;
using ITTitans.Hackathon2025.WebAPI.Model.User;
using ITTitans.Hackathon2025.WebAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.WebAPI.Endpoints.Skill;

public class UpdateSkillReviewersEndpoint : Endpoint<UpdateSkillReviewersBindingModel, SkillDetailBindingModel>
{
    private readonly HackathonDbContext dbContext;
    private readonly UserManager<HackathonUserEntity> userManager;

    public UpdateSkillReviewersEndpoint(HackathonDbContext dbContext, UserManager<HackathonUserEntity> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    public override void Configure()
    {
        this.Put("api/skills/{id:guid}/reviewers");
        this.AddHackathonPolicy(AuthClaimType.ManageSkill);

        this.Description(builder => builder
            .WithName("UpdateSkillReviewers")
            .WithTags("Skills"));
    }

    public override async Task HandleAsync(UpdateSkillReviewersBindingModel req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        var routeId = this.Route<Guid>("id");
        if (routeId != req.SkillId)
        {
            this.AddError("SkillId in route does not match request body.");
            await this.Send.ErrorsAsync(cancellation: ct);
            return;
        }

        SkillEntity? skill = await this.dbContext.Skills
            .Include(s => s.PossibleReviewer)
            .FirstOrDefaultAsync(s => s.Id == req.SkillId && !s.IsDeleted, ct);

        if (skill is null)
        {
            await this.Send.NotFoundAsync(ct);
            return;
        }

        // Load all users to assign
        HashSet<Guid> desiredReviewerIds = req.ReviewerUserIds.Distinct().ToHashSet();
        List<HackathonUserEntity> desiredUsers = await this.userManager.Users
            .Where(u => !u.IsDeleted && desiredReviewerIds.Contains(u.Id))
            .ToListAsync(ct);

        // Remove users no longer desired
        List<HackathonUserEntity> toRemove = skill.PossibleReviewer.Where(u => !desiredReviewerIds.Contains(u.Id)).ToList();
        foreach (HackathonUserEntity user in toRemove)
        {
            skill.PossibleReviewer.Remove(user);
        }

        // Add missing users
        HashSet<Guid> existingIds = skill.PossibleReviewer.Select(u => u.Id).ToHashSet();
        foreach (HackathonUserEntity user in desiredUsers)
        {
            if (!existingIds.Contains(user.Id))
            {
                skill.PossibleReviewer.Add(user);
            }
        }

        await this.dbContext.SaveChangesAsync(ct);

        // Return detailed model
        var result = new SkillDetailBindingModel
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description,
            PossibleReviewers = skill.PossibleReviewer
                .Where(u => !u.IsDeleted)
                .Select(u => new BasicUserBindingModel
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    DisplayName = u.DisplayName,
                })
        };

        await this.Send.OkAsync(result, ct);
    }
}
