using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.Hackathon2025.EntityModel;

public class HackathonDbContext : IdentityDbContext<HackathonUserEntity, HackathonRoleEntity, Guid>
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options) : base(options)
    {
    }
}
