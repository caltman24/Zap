using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class AddMember : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle)
        .Accepts<Request>("application/json")
        .WithRequestValidation<Request>();

    public record Request(IEnumerable<string> memberIds);
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.memberIds).NotEmpty();
        }
    }

    public static async Task<Results<BadRequest<string>, NoContent>> Handle(
            [FromRoute] string projectId,
            [FromBody] Request request,
            IProjectService projectService)
    {
        var success = await projectService.AddMembersToProjectAsync(projectId, request.memberIds);
        if (success) return TypedResults.NoContent();

        return TypedResults.BadRequest("Failed to add members to project");
    }

}
