using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;

namespace Zap.Api.Features.Users.Endpoints;

public class GetUserInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/info", Handle);

    public record Response(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string Role,
        string AvatarUrl,
        string? CompanyId);

    private static Results<BadRequest<string>, Ok<Response>> Handle(
        HttpContext context, CurrentUser currentUser)
    {
        var user = currentUser.User;
        if (user == null) return TypedResults.BadRequest("User not found");

        var response = new Response
        (
            Id: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AvatarUrl: user.AvatarUrl,
            Role: currentUser.Role,
            CompanyId: currentUser.CompanyId
        );

        return TypedResults.Ok(response);
    }
}
