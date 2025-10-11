using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

public class ServerDbContext : IdentityDbContext<IdentityUser>
{
    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }
        
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "guest-role-id",
                Name = "guest",
                NormalizedName = "GUEST"
            }
        );
    }
}