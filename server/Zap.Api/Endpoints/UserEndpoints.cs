using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Authorization;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

internal static class UserEndpoints
{
    internal static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user");

        group.MapGet("/info", GetUserInfoHandler);

        return app;
    }

    private static Results<BadRequest<string>, Ok<UserInfoResponse>> GetUserInfoHandler(
        HttpContext context, CurrentUser currentUser)
    {
        var user = currentUser.User;
        if (user == null) return TypedResults.BadRequest("User not found");

        var response = new UserInfoResponse
        (
            Id: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AvatarUrl: user.AvatarUrl,
            Role: currentUser.Role,
            CompanyId: user.CompanyId
        );

        return TypedResults.Ok(response);
    }
}

internal record UserInfoResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AvatarUrl,
    string? CompanyId);