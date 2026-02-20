using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class UpdateComment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{ticketId}/comments/{commentId}", Handle)
            .WithCompanyMember()
            .WithTicketCompanyValidation()
            .WithTicketArchiveValidation()
            .WithRequestValidation<Request>();
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        [FromRoute] string ticketId,
        [FromRoute] string commentId,
        CurrentUser currentUser,
        ITicketCommentsService commentsService,
        Request request
    )
    {
        var success = await commentsService.UpdateCommentAsync(
            ticketId,
            commentId,
            request.Message,
            currentUser.Member!.Id);

        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

    public record Request(string Message);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Message).NotEmpty().NotNull().MaximumLength(150);
        }
    }
}
