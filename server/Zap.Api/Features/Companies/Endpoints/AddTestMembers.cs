using Bogus;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Companies.Endpoints;

class AddTestMembers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/testmembers", Handle);

    private static async Task<Results<BadRequest<string>, NoContent>> Handle(
            [FromQuery] int count,
            UserManager<AppUser> userManager,
            CurrentUser currentUser,
            [FromQuery] string? role = null)
    {
        var companyId = currentUser.CompanyId;
        if (companyId == null) return TypedResults.BadRequest("User not in a company");

        var newUsers = new Faker<AppUser>()
            .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.AvatarUrl, (f, u) => f.Internet.Avatar())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
            .RuleFor(u => u.EmailConfirmed, (f, u) => true)
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.CompanyId, (f, u) => companyId)
            .Generate(count);

        List<string> roles = [RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Developer, RoleNames.Submitter];

        foreach (var user in newUsers)
        {
            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role ?? roles[Random.Shared.Next(0, roles.Count() - 1)]);
            }
        }

        return TypedResults.NoContent();
    }
}
