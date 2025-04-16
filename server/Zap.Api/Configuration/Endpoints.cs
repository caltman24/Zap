﻿using Zap.Api.Common;
using Zap.Api.Features.Authentication.Endpoints;
using Zap.Api.Features.Companies.Endpoints;
using Zap.Api.Features.Projects.Endpoints;
using Zap.Api.Features.Users.Endpoints;

namespace Zap.Api.Configuration;

public static class Endpoints
{
    public static void MapZapApiEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints(app.Environment)
            .MapCompaniesEndpoints()
            .MapProjectsEndpoints()
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

    private static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company")
            .WithTags("Company");

        group.MapEndpoint<GetCompanyInfo>()
            .MapEndpoint<UpdateCompanyInfo>()
            .MapEndpoint<RegisterCompany>();

        group.MapGroup("/projects")
            .WithTags("CompanyProjects")
            .MapEndpoint<GetCompanyProjects>();


        return app;
    }

    private static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
            .WithTags("Projects");

        group.MapEndpoint<GetProject>()
            .MapEndpoint<CreateProject>()
            .MapEndpoint<ArchiveProject>()
            .MapEndpoint<UpdateProject>();

        group.MapGroup("/{projectId}/members")
            .WithTags("ProjectMembers")
            .MapEndpoint<GetUnassignedCompanyMembers>();

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
