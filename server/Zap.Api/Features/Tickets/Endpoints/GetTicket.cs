using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{ticketId}", Handle)
            .WithName("GetTicket")
            .WithCompanyMember()
            .WithTicketAccessValidation();
    }

    private static async Task<Results<NotFound, Ok<Response>>> Handle(
        [FromRoute] string ticketId,
        ITicketService ticketService,
        ITicketAuthorizationService ticketAuthorizationService,
        CurrentUser currentUser
    )
    {
        var ticket = await ticketService.GetTicketByIdAsync(ticketId);

        if (ticket == null) return TypedResults.NotFound();

        var capabilities = ticketAuthorizationService.GetCapabilities(ticket, currentUser);

        return TypedResults.Ok(new Response(
            ticket.Id,
            ticket.DisplayId,
            ticket.Name,
            ticket.Description,
            ticket.Priority,
            ticket.Status,
            ticket.Type,
            ticket.ProjectId,
            ticket.ProjectManagerId,
            ticket.isArchived,
            ticket.ProjectIsArchived,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.Submitter,
            ticket.Assignee,
            capabilities));
    }

    private record Response(
        string Id,
        string DisplayId,
        string Name,
        string Description,
        string Priority,
        string Status,
        string Type,
        string ProjectId,
        string? ProjectManagerId,
        bool isArchived,
        bool ProjectIsArchived,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        MemberInfoDto Submitter,
        MemberInfoDto? Assignee,
        TicketCapabilitiesDto Capabilities);
}
