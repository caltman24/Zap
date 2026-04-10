using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Companies.Endpoints;

/// <summary>
///     Searches visible active tickets and projects for the current company member.
/// </summary>
    public class GetCompanySearch : IEndpoint
    {
        private const int ResultLimitPerType = 5;
        private const int MinimumQueryLength = 2;

    /// <summary>
    ///     Maps the company search endpoint.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/search", Handle)
            .WithCompanyMember();
    }

    /// <summary>
    ///     Handles a combined company search request.
    /// </summary>
    /// <param name="companyService">The company service.</param>
    /// <param name="ticketService">The ticket service.</param>
    /// <param name="currentUser">The current user context.</param>
    /// <param name="query">The raw query string.</param>
    /// <returns>A combined list of project and ticket search results.</returns>
    private static async Task<Ok<List<Response>>> Handle(
        ICompanyService companyService,
        ITicketService ticketService,
        CurrentUser currentUser,
        [FromQuery] string? query = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return TypedResults.Ok<List<Response>>([]);
        }

        var currentMember = currentUser.Member!;
        var trimmedQuery = query.Trim();

        if (trimmedQuery.Length < MinimumQueryLength)
        {
            return TypedResults.Ok<List<Response>>([]);
        }

        var projects = await companyService.SearchVisibleProjectsAsync(
            currentUser.CompanyId!,
            currentMember.Id,
            currentMember.Role.Name,
            trimmedQuery,
            ResultLimitPerType);
        var tickets = await ticketService.SearchVisibleTicketsAsync(
            currentMember.Id,
            currentMember.Role.Name,
            currentUser.CompanyId!,
            trimmedQuery,
            ResultLimitPerType);

        var results = projects
            .Select(project => new Response(
                "project",
                project.Id,
                null,
                project.Name,
                null))
            .Concat(tickets.Select(ticket => new Response(
                "ticket",
                ticket.Id,
                ticket.ProjectId,
                ticket.Name,
                ticket.DisplayId)))
            .OrderBy(result => result.Name)
            .ToList();

        return TypedResults.Ok(results);
    }

    private record Response(
        string Type,
        string Id,
        string? ProjectId,
        string Name,
        string? DisplayId);
}
