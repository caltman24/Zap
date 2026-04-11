using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Users.Services;

namespace Zap.Api.Features.Users.Endpoints;

public class GetUserInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/info", Handle);
    }

    private static Results<NotFound<string>, Ok<Response>> Handle(
        HttpContext context, CurrentUser currentUser, IUserPermissionService userPermissionService)
    {
        var user = currentUser.User;
        if (user == null) return TypedResults.NotFound("User not found");

        var permissions = userPermissionService.GetPermissions(currentUser);

        var response = new Response
        (
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            AvatarUrl: user.AvatarUrl,
            Role: currentUser.Member?.Role?.Name ?? "",
            CompanyId: currentUser.CompanyId,
            MemberId: currentUser.Member?.Id,
            Permissions: permissions
        );

        return TypedResults.Ok(response);
    }

    public record Response(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string Role,
        string AvatarUrl,
        string? CompanyId,
        string? MemberId,
        IReadOnlyList<string> Permissions);
}