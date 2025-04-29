using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Data.Models;

namespace Zap.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyMember> CompanyMembers { get; set; }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>()
            .HasOne(u => u.Owner)
            .WithOne()
            .HasForeignKey<Company>(c => c.OwnerId);

        builder.Entity<CompanyMember>()
            .HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<CompanyMember>(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CompanyMember>()
            .HasOne(u => u.Company)
            .WithMany(c => c.Members)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
