using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Zap.Api.Endpoints;
using Zap.DataAccess;
using Zap.DataAccess.Models;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddOpenApi();


    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    builder.Services.AddIdentityCore<AppUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager<SignInManager<AppUser>>();

    builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
        .AddBearerToken(IdentityConstants.BearerScheme);

    builder.Services.AddAuthorizationBuilder()
        .AddDefaultPolicy("default", pb =>
        {
            pb.RequireAuthenticatedUser();
            pb.Build();
        });

    builder.Services.AddCors(opts =>
    {
        opts.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://client.scalar.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRegisterUserEndpoints().MapSignInEndpoints();

    app.MapGet("/company",
        async (AppDbContext db, HttpContext context) =>
        {
            return Results.Ok(context.User.Claims.Select(c => c.Value));
        }).RequireAuthorization(pb => pb.RequireRole("Admin"));

    app.Run();
}
