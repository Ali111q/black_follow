using black_follow.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace black_follow.Data;

public class DataContext:IdentityDbContext<AppUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

}