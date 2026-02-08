using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetComments : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{ticketId}/comments", Handle)
            .WithCompanyMember()
            .WithTicketCompanyValidation();
    }

    private static async Task<Ok<List<CommentDto>>> Handle(
        [FromRoute] string ticketId,
        CurrentUser currentUser,
        ITicketCommentsService commentsService
    )
    {
        var comments = await commentsService.GetCommentsAsync(ticketId);
        return TypedResults.Ok(comments);
    }
}