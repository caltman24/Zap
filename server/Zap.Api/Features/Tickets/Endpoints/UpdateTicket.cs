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

public class UpdateTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/type", Handle)
            .WithName("UpdateTicket")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Submitter)
            .WithTicketCompanyValidation()
            .WithRequestValidation<Request>();

    public record Request(
        string Name,
        string Description,
        string Priority,
        string Status,
        string Type
    );

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(50);
            RuleFor(x => x.Description).NotNull().NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Priority).ValidateTicketPriority();
            RuleFor(x => x.Status).ValidateTicketStatus();
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

        var success = await ticketService.UpdateTicketAsync(ticketId, new UpdateTicketDto(
                    "",
                    "",
                    "",
                    "",
                    ""));
        if (!success) return TypedResults.Problem();

        return TypedResults.NoContent();
    }
}

