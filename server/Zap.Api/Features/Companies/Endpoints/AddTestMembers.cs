using Bogus;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Companies.Endpoints;

class AddTestMembers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/testmembers", Handle);

    private static async Task<Results<BadRequest<string>, NoContent>> Handle(
            [FromQuery] int count,
            UserManager<AppUser> userManager,
            AppDbContext db,
            CurrentUser currentUser,
            [FromQuery] string? role = null)
    {
        var companyId = currentUser.CompanyId;
        if (companyId == null) return TypedResults.BadRequest("User not in a company");

        var company = await db.Companies
            .Where(c => c.Id == companyId)
            .Include(c => c.Members)
            .FirstOrDefaultAsync();
        if (company == null) return TypedResults.BadRequest("User not in a company");

        var newUsers = new Faker<AppUser>()
            .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.AvatarUrl, (f, u) => f.Internet.Avatar())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
            .RuleFor(u => u.EmailConfirmed, (f, u) => true)
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .Generate(count);

        List<CompanyRole> roles = await db.CompanyRoles.ToListAsync();

        foreach (var user in newUsers)
        {
            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                company.Members.Add(new CompanyMember
                {
                    UserId = user.Id,
                    CompanyId = companyId,
                    RoleId = roles.FirstOrDefault(r => r.Name == role)?.Id ?? roles[Random.Shared.Next(0, roles.Count() - 1)].Id

                });
                await db.SaveChangesAsync();
            }
        }

        return TypedResults.NoContent();
    }
}
