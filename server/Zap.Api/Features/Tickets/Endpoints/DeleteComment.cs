using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class DeleteComment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{ticketId}/comments/{commentId}", Handle)
            .WithCompanyMember()
            .WithTicketCompanyValidation()
            .WithTicketArchiveValidation();

    private static async Task<Results<NoContent, NotFound>> Handle(
            [FromRoute] string ticketId,
            [FromRoute] string commentId,
            CurrentUser currentUser,
            ITicketCommentsService commentsService
            )
    {
        var success = await commentsService.DeleteCommentAsync(commentId, currentUser.Member!.Id);
        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }
}
