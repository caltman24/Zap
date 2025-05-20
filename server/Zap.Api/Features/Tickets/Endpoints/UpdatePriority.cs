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

public class UpdatePriority : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/priority", Handle)
            .WithName("UpdateTicketPriority")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Submitter)
            .WithTicketCompanyValidation()
            .WithRequestValidation<Request>();

    public record Request(string Priority);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Priority).ValidateTicketPriority();
        }
    }

    private static async Task<Results<ForbidHttpResult, ProblemHttpResult, NoContent>> Handle(
            [FromRoute] string ticketId,
            Request request,
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var validMember = currentUser.Role switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager =>
                await ticketService.ValidateProjectManagerAsync(ticketId, currentUser.Member!.Id),
            RoleNames.Submitter =>
                await ticketService.ValidateAssignedMemberAsync(ticketId, currentUser.Member!.Id),
            _ => false
        };

        if (!validMember) return TypedResults.Forbid();

        var success = await ticketService.UpdatePriorityAsync(ticketId, request.Priority);
        if (!success) return TypedResults.Problem();

        return TypedResults.NoContent();
    }
}

