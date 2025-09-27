using ITTitans.Hackathon2025.EntityModel.Attachment;
using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.EntityModel.Skill;
using ITTitans.Hackathon2025.EntityModel.Workflow.SupplyCertificate;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.EntityModel;

public class HackathonDbContext : IdentityDbContext<HackathonUserEntity, HackathonRoleEntity, Guid>
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options) : base(options)
    {
    }

    public DbSet<SkillEntity> Skills => this.Set<SkillEntity>();

    public DbSet<SkillReviewConfigurationEntity> SkillReviewConfigurations => this.Set<SkillReviewConfigurationEntity>();

    public DbSet<SkillAssignmentEntity> SkillAssignments => this.Set<SkillAssignmentEntity>();

    #region workflow

    public DbSet<SupplyCertificateWorkflowEntity> SupplyCertificateWorkflows => this.Set<SupplyCertificateWorkflowEntity>();

    #endregion
    
    #region attachment

    public DbSet<AttachmentEntity> Attachments => this.Set<AttachmentEntity>();
    
    public DbSet<AttachmentLinkEntity> AttachmentLinks => this.Set<AttachmentLinkEntity>();

    #endregion
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        ArgumentNullException.ThrowIfNull(builder);
        
        builder
            .Entity<HackathonUserEntity>()
            .HasMany(x => x.PossibleSkillsToReview)
            .WithMany(x => x.PossibleReviewer)
            .UsingEntity<SkillReviewConfigurationEntity>();

        builder
            .Entity<HackathonUserEntity>()
            .HasMany(x => x.InitiatedSupplyCertificateWorkflows)
            .WithOne(x => x.Initiator);
        builder
            .Entity<HackathonUserEntity>()
            .HasMany(x => x.ReviewedSupplyCertificateWorkflows)
            .WithOne(x => x.Reviewer);
    }
}
