using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class UpdateAssignee : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{ticketId}/developer", Handle)
            .WithName("UpdateDeveloper")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithTicketCompanyValidation()
            .WithTicketArchiveValidation();
    }

    private static async Task<Results<NotFound, BadRequest<string>, ForbidHttpResult, NoContent>> Handle(
        [FromRoute] string ticketId,
        Request? request,
        ITicketService ticketService,
        CurrentUser currentUser
    )
    {
        var memberId = request?.MemberId ?? request?.DeveloperId;

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
}
