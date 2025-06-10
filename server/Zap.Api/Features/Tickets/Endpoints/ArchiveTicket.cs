using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
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


    private static async Task<Results<ForbidHttpResult, NoContent, NotFound, BadRequest<string>>> Handle(
            [FromRoute] string ticketId,
            CurrentUser currentUser,
            ITicketService ticketService,
            AppDbContext db
            )
    {
        var userRole = currentUser.Member!.Role.Name;

        if (userRole == RoleNames.ProjectManager)
        {
            var validPM = await ticketService.ValidateProjectManagerAsync(ticketId, currentUser.Member!.Id);
            if (!validPM) return TypedResults.Forbid();
        }

        // Check if we're trying to unarchive a ticket and if the project is archived
        var ticketInfo = await db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => new { t.IsArchived, ProjectIsArchived = t.Project.IsArchived })
            .FirstOrDefaultAsync();

        if (ticketInfo == null) return TypedResults.NotFound();

        // If the ticket is currently archived (meaning we're trying to unarchive it)
        // and the project is archived, prevent the operation
        if (ticketInfo.IsArchived && ticketInfo.ProjectIsArchived)
        {
            return TypedResults.BadRequest("Cannot unarchive a ticket when its project is archived. Please unarchive the project first.");
        }

        var success = await ticketService.ToggleArchiveTicket(ticketId, currentUser.Member!.Id);
        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }
}

