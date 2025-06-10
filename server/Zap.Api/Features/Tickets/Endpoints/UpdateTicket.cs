using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Extensions;
using Zap.Api.Common.Filters;
using Zap.Api.Data;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class UpdateTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}", Handle)
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

    private static async Task<Results<ForbidHttpResult, ProblemHttpResult, NoContent, BadRequest<string>, NotFound<string>>> Handle(
            [FromRoute] string ticketId,
            Request request,
            ITicketService ticketService,
            CurrentUser currentUser,
            AppDbContext db
            )
    {
        // Check if ticket is archived
        var ticket = await db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => new
            {
                t.IsArchived,
                t.Name,
                t.Description,
                PriorityName = t.Priority.Name,
                StatusName = t.Status.Name,
                TypeName = t.Type.Name
            })
            .FirstOrDefaultAsync();

        if (ticket == null)
        {
            return TypedResults.NotFound("Ticket not found");
        }

        // If ticket is archived, only allow name and description updates
        if (ticket.IsArchived)
        {
            // Check if priority, status, or type are being changed
            if (request.Priority != ticket.PriorityName ||
                request.Status != ticket.StatusName ||
                request.Type != ticket.TypeName)
            {
                return TypedResults.BadRequest("Archived tickets can only have their name and description updated.");
            }

            // Only update name and description for archived tickets
            var success = await ticketService.UpdateArchivedTicketAsync(
                ticketId,
                request.Name,
                request.Description,
                currentUser.Member!.Id);

            if (!success) return TypedResults.Problem();
            return TypedResults.NoContent();
        }

        // For non-archived tickets, allow full updates
        var fullUpdateSuccess = await ticketService.UpdateTicketAsync(ticketId, new UpdateTicketDto(
                    request.Name,
                    request.Description,
                    request.Priority,
                    request.Status,
                    request.Type), currentUser.Member!.Id);
        if (!fullUpdateSuccess) return TypedResults.Problem();

        return TypedResults.NoContent();
    }
}

