using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Extensions;
using Zap.Api.Common.Filters;
using Zap.Api.Data;
using Zap.Api.Features.Projects.Services;
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
            RuleFor(r => r.Priority).ValidateTicketPriority();
            RuleFor(r => r.Status).ValidateTicketStatus();
            RuleFor(r => r.Type).ValidateTicketType();
            RuleFor(r => r.ProjectId).NotNull().NotEmpty().MaximumLength(100);
        }
    }

    private static async Task<Results<ForbidHttpResult, CreatedAtRoute<CreateTicketResult>, BadRequest<string>, NotFound<string>>> Handle(
            Request request,
            CurrentUser currentUser,
            ITicketService ticketService,
            IProjectService projectService,
            AppDbContext db
            )
    {
        var userRole = currentUser.Member!.Role.Name;

        // Check if the project is archived
        var project = await db.Projects
            .Where(p => p.Id == request.ProjectId)
            .Select(p => new { p.IsArchived })
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return TypedResults.NotFound("Project not found");
        }

        if (project.IsArchived)
        {
            return TypedResults.BadRequest("Cannot create tickets in an archived project.");
        }

        // INFO: Can only create a ticket if user is admin of the requesting project's company
        // OR if the user is an assigned member to the requesting project
        if (userRole == RoleNames.ProjectManager)
        {
            var isAssignedProjectMember = await projectService.ValidateAssignedMemberAsync(request.ProjectId, currentUser.Member!.Id);
            if (!isAssignedProjectMember) return TypedResults.Forbid();
        }


        var newTicket = await ticketService.CreateTicketAsync(new CreateTicketDto(
            request.Name,
            request.Description,
            request.Priority,
            request.Status,
            request.Type,
            request.ProjectId,
            currentUser.Member!.Id
            ), currentUser.Member!.Id);

        return TypedResults.CreatedAtRoute(newTicket, "GetTicket", new { TicketId = newTicket.Id });
    }
}

