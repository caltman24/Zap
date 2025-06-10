using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Extensions;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class UpdateType : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/type", Handle)
            .WithName("UpdateTicketType")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Submitter)
            .WithTicketCompanyValidation()
            .WithTicketArchiveValidation()
            .WithRequestValidation<Request>();

    public record Request(string Type);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Type).ValidateTicketType();
        }
    }

    private static async Task<Results<ForbidHttpResult, ProblemHttpResult, NoContent>> Handle(
            [FromRoute] string ticketId,
            Request request,
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var success = await ticketService.UpdateTypeAsync(ticketId, request.Type, currentUser.Member!.Id);
        if (!success) return TypedResults.Problem();

        return TypedResults.NoContent();
    }
}

