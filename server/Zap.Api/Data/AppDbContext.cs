﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Data.Models;

namespace Zap.Api.Data;

public class AppDbContext : IdentityUserContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; } = default!;
    public DbSet<CompanyMember> CompanyMembers { get; set; } = default!;
    public DbSet<CompanyRole> CompanyRoles { get; set; } = default!;

    public DbSet<Project> Projects { get; set; } = default!;
    public DbSet<Ticket> Tickets { get; set; } = default!;

    public DbSet<TicketPriority> TicketPriorities { get; set; } = default!;
    public DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
    public DbSet<TicketType> TicketTypes { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>()
            .HasOne(u => u.Owner)
            .WithOne()
            .HasForeignKey<Company>(c => c.OwnerId);


        // Company Member
        builder.Entity<CompanyMember>()
            .HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<CompanyMember>(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CompanyMember>()
            .HasOne(u => u.Company)
            .WithMany(c => c.Members)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CompanyMember>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CompanyMember>()
            .HasMany(cm => cm.AssignedProjects)
            .WithMany(ap => ap.AssignedMembers)
            .UsingEntity(j => j.ToTable("ProjectMembers"));


        // Project
        builder.Entity<Project>()
            .HasMany(ap => ap.AssignedMembers)
            .WithMany(cm => cm.AssignedProjects)
            .UsingEntity(j => j.ToTable("ProjectMembers"));

        builder.Entity<Project>()
            .HasOne(p => p.ProjectManager)
            .WithMany()
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.SetNull);


        // Ticket
        builder.Entity<Ticket>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tickets)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ticket>()
            .HasOne(t => t.Priority)
            .WithMany()
            .HasForeignKey(t => t.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Type)
            .WithMany()
            .HasForeignKey(t => t.TypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
