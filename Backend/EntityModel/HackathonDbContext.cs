using ITTitans.Hackathon2025.EntityModel.Auth;
using ITTitans.Hackathon2025.EntityModel.Skill;
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
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder
            .Entity<HackathonUserEntity>()
            .HasMany(x => x.PossibleSkillsToReview)
            .WithMany(x => x.PossibleReviewer)
            .UsingEntity<SkillReviewConfigurationEntity>();
    }
}
