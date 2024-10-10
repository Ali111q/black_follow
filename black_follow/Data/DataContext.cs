using black_follow.Entity;
using Microsoft.EntityFrameworkCore;

namespace black_follow.Data;

public class DataContext:DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
}