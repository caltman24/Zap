using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class CreateProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle)
            .WithName("CreateProject")
            .RequireAuthorization(pb =>
            {
                pb.RequireCurrentUser();
                pb.RequireCompanyMember(RoleNames.Admin, RoleNames.ProjectManager);
                pb.Build();
            })
            .WithRequestValidation<Request>();

    public record Request(string Name, string Description, string Priority, DateTime DueDate);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(50);
            RuleFor(x => x.Description).NotEmpty().NotNull().MaximumLength(1000);
            RuleFor(x => x.Priority).NotEmpty().NotNull().MaximumLength(50);
            RuleFor(x => x.DueDate).NotNull().GreaterThan(DateTime.UtcNow);
        }
    }


    private static async Task<Results<BadRequest<string>, CreatedAtRoute<ProjectDto>>> Handle(
        Request request, IProjectService projectService, CurrentUser currentUser, ILogger<Program> logger)
    {
        if (currentUser.Member == null) return TypedResults.BadRequest("User not in company");

        var newProject = await projectService.CreateProjectAsync(new CreateProjectDto
        (
            Name: request.Name,
            Description: request.Description,
            Priority: request.Priority,
            DueDate: request.DueDate,
            Member: currentUser.Member
        ));

        return TypedResults.CreatedAtRoute(newProject, "GetProject", new { ProjectId = newProject.Id });
    }
}
