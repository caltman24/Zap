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
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/developer", Handle)
            .WithName("UpdateDeveloper")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithTicketCompanyValidation();

    public record Request(string MemberId);

    private static async Task<Results<NotFound, NoContent>> Handle(
            [FromRoute] string ticketId,
            Request request,
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var valid = currentUser.Role == RoleNames.Admin ||
            await ticketService.ValidateProjectManagerAsync(ticketId, currentUser.Member!.Id);
        // validate projectManager
        var success = await ticketService.UpdateAsigneeAsync(ticketId, request.MemberId);

        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }
}

