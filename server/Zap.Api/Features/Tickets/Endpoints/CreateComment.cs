using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class CreateComment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/{ticketId}/comments", Handle)
            .WithCompanyMember()
            .WithTicketCompanyValidation()
            .WithTicketArchiveValidation()
            .WithRequestValidation<Request>();

    public record Request(string Message);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Message).NotEmpty().NotNull().MaximumLength(150);
        }
    }

    private async static Task<NoContent> Handle(
            [FromRoute] string ticketId,
            CurrentUser currentUser,
            ITicketCommentsService commentsService,
            Request request
            )
    {
        await commentsService.CreateCommentAsync(
                currentUser.Member!.Id,
                ticketId,
                request.Message);

        return TypedResults.NoContent();
    }
}

