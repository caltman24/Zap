using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class CreateTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Submitter)
            .WithRequestValidation<Request>();

    public record Request(
            string Name,
            string Description,
            string Priority,
            string Status,
            string Type,
            string ProjectId
    );

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name).NotNull().NotEmpty().MaximumLength(50);
            RuleFor(r => r.Description).NotNull().NotEmpty().MaximumLength(1000);
            RuleFor(r => r.Priority).NotNull().NotEmpty().MaximumLength(50);
            RuleFor(r => r.Status).NotNull().NotEmpty().MaximumLength(50);
            RuleFor(r => r.Type).NotNull().NotEmpty().MaximumLength(50);
            RuleFor(r => r.ProjectId).NotNull().NotEmpty().MaximumLength(100);
        }
    }

    private static async Task<CreatedAtRoute<CreateTicketResult>> Handle(
            Request request,
            CurrentUser currentUser,
            ITicketService ticketService
            )
    {
        // TODO: Validate Priority, Status, and Type exist in db. Return validation error if not
        var newTicket = await ticketService.CreateTicketAsync(new CreateTicketDto(
            request.Name,
            request.Description,
            request.Priority,
            request.Status,
            request.Type,
            request.ProjectId,
            currentUser.Member!.Id
            ));

        return TypedResults.CreatedAtRoute(newTicket, "GetTicket", new { TicketId = newTicket.Id });
    }
}

