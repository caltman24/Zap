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

public class UpdateStatus : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/status", Handle)
            .WithName("UpdateTicketStatus")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Submitter)
            .WithTicketCompanyValidation()
            .WithTicketArchiveValidation()
            .WithRequestValidation<Request>();

    public record Request(string Status);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Status).ValidateTicketStatus();
        }
    }

    private static async Task<Results<ForbidHttpResult, ProblemHttpResult, NoContent>> Handle(
            [FromRoute] string ticketId,
            Request request,
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var success = await ticketService.UpdateStatusAsync(ticketId, request.Status, currentUser.Member!.Id);
        if (!success) return TypedResults.Problem();

        return TypedResults.NoContent();
    }
}

