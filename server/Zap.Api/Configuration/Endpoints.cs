using Zap.Api.Common;
using Zap.Api.Features.Authentication.Endpoints;
using Zap.Api.Features.Companies.Endpoints;
using Zap.Api.Features.Members.Endpoints;
using Zap.Api.Features.Projects.Endpoints;
using Zap.Api.Features.Tickets;
using Zap.Api.Features.Users.Endpoints;

namespace Zap.Api.Configuration;

public static class Endpoints
{
    public static void MapZapApiEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints(app.Environment);

        app.MapCompaniesEndpoints(app.Environment)
            .MapProjectsEndpoints()
            .MapTicketsEndpoints()
            .MapMembersEndpoints()
            .MapUsersEndpoints();
    }

    private static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app,
        IWebHostEnvironment env)
    {
        var publicGroup = app.MapPublicGroup("/auth")
            .WithTags("Authentication");

        if (env.IsDevelopment()) publicGroup.MapEndpoint<SignInTestUser>();

        publicGroup.MapEndpoint<RegisterUser>()
            .MapEndpoint<SignInUser>()
            .MapEndpoint<RefreshTokens>();

        return app;
    }

    private static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder app, IWebHostEnvironment env)
    {
        var group = app.MapGroup("/company")
            .WithTags("Company");

        group.MapEndpoint<GetCompanyInfo>()
            .MapEndpoint<UpdateCompanyInfo>()
            .MapEndpoint<RegisterCompany>();

        group.MapGroup("/projects")
            .WithTags("CompanyProjects")
            .MapEndpoint<GetCompanyProjects>();

        if (env.IsDevelopment()) group.MapEndpoint<AddTestMembers>();

        return app;
    }

    private static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
            .WithTags("Projects");

        group.MapEndpoint<GetProject>()
            .MapEndpoint<CreateProject>()
            .MapEndpoint<ArchiveProject>()
            .MapEndpoint<UpdateProject>()
            .MapEndpoint<AssignProjectManager>()
            .MapEndpoint<GetAssignableProjectManagers>();

        group.MapGroup("/{projectId}/members")
            .MapEndpoint<AddMembers>()
            .MapEndpoint<GetUnassignedCompanyMembers>()
            .MapEndpoint<RemoveMember>();


        return app;
    }

    private static IEndpointRouteBuilder MapTicketsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tickets")
            .WithTags("Tickets");

        group.MapEndpoint<CreateTicket>()
            .MapEndpoint<CreateTicketProjectList>()
            .MapEndpoint<GetTicket>()
            .MapEndpoint<GetMyTickets>()
            .MapEndpoint<GetOpenTickets>()
            .MapEndpoint<GetArchivedTickets>()
            .MapEndpoint<GetResolvedTickets>()
            .MapEndpoint<GetDeveloperList>()
            .MapEndpoint<DeleteTicket>()
            .MapEndpoint<UpdateAssignee>()
            .MapEndpoint<UpdatePriority>()
            .MapEndpoint<UpdateStatus>()
            .MapEndpoint<UpdateType>()
            .MapEndpoint<UpdateTicket>()
            .MapEndpoint<ArchiveTicket>()
            .MapEndpoint<GetTicketHistory>()
            .MapEndpoint<GetTicketHistoryPaginated>();

        group.MapEndpoint<CreateComment>()
            .MapEndpoint<GetComments>()
            .MapEndpoint<DeleteComment>()
            .MapEndpoint<UpdateComment>();


        return app;
    }

    private static IEndpointRouteBuilder MapMembersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/members")
            .WithTags("CompanyMembers");

        group.MapEndpoint<GetMyProjects>();

        return app;
    }

    private static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user")
            .WithTags("User");

        group.MapEndpoint<GetUserInfo>();

        return app;
    }

    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? "")
            .AllowAnonymous();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}