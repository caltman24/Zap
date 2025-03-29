using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user");

        group.MapGet("/info", GetUserInfoHandler);

        return app;
    }

    private static async Task<Results<BadRequest<string>, Ok<UserInfoResponse>>> GetUserInfoHandler(
        HttpContext context, UserManager<AppUser> userManager)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null) return TypedResults.BadRequest("User not found");

        var response = new UserInfoResponse
        (
            Id: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AvatarUrl: user.AvatarUrl,
            Role: context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "None",
            CompanyId: user.CompanyId
        );

        return TypedResults.Ok(response);
    }
}

record UserInfoResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AvatarUrl,
    string? CompanyId);