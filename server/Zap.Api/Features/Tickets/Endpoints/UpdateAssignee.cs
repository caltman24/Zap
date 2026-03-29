using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class UpdateAssignee : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{ticketId}/developer", Handle)
            .WithName("UpdateDeveloper")
            .WithRequestValidation<Request>()
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithTicketAccessValidation()
            .WithActiveTicketValidation();
    }

    private static async Task<Results<NotFound, BadRequest<string>, ForbidHttpResult, NoContent>> Handle(
        [FromRoute] string ticketId,
        Request request,
        ITicketService ticketService,
        CurrentUser currentUser,
        ITicketAuthorizationService ticketAuthorizationService
    )
    {
        if (!await ticketAuthorizationService.CanAssignDeveloperAsync(ticketId, currentUser))
            return TypedResults.Forbid();

        var memberId = request.MemberId ?? request.DeveloperId;

        if (memberId != null)
        {
            // Make sure the requesting assigne id is an assigned member of the ticket's project
            var validAssignee = await ticketService.ValidateAssigneeAsync(ticketId, memberId);
            if (!validAssignee)
                return TypedResults.BadRequest(
                    "Member of AssigneeId is not an assigned developer of the ticket's project");
        }

        // We can allow a null memberId, this removes the assignee
        var success = await ticketService.UpdateAsigneeAsync(ticketId, memberId, currentUser.Member!.Id);

        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

    public record Request(string? MemberId, string? DeveloperId);
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x)
                .Must(x =>
                    x.MemberId == null ||
                    x.DeveloperId == null ||
                    x.MemberId == x.DeveloperId)
                .WithMessage("Provide only one assignee field, or make both values the same.");
        }
    }
}
