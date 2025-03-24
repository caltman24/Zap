using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.DataAccess;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class CompanyEndpoints
{
    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company");

        group.MapGet("/info", async (AppDbContext db, UserManager<AppUser> userManager, HttpContext context) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null) return Results.BadRequest("User not found");
            
            var company = await db.Companies.FindAsync(user.CompanyId);
            if (company == null) return Results.BadRequest("Company not found");

            return Results.Ok(new CompanyInfoResponse(company.Name, company.Description));
        }).RequireAuthorization();

        return app;
    }
}

public record CompanyInfoResponse(string Name, string Description);