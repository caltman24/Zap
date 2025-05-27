using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Services;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class ArchiveTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/archive", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithTicketCompanyValidation();


    private static async Task<Results<ForbidHttpResult, NoContent, NotFound>> Handle(
            [FromRoute] string ticketId,
            CurrentUser currentUser,
            ITicketService ticketService
            )
    {
        var userRole = currentUser.Member!.Role.Name;

        if (userRole == RoleNames.ProjectManager)
        {
            var validPM = await ticketService.ValidateProjectManagerAsync(ticketId, currentUser.Member!.Id);
            if (!validPM) return TypedResults.Forbid();
        }

        var success = await ticketService.ToggleArchiveTicket(ticketId);
        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }
}

