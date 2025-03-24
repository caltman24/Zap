using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

    app.MapUserEndpoints().MapCompanyEndpoints();

    app.MapPost("/refresh",
        async (RefreshTokenRequest request, SignInManager<AppUser> signInManager,
            IOptionsMonitor<BearerTokenOptions> bearerTokenOptions, TimeProvider timeProvider) =>
        {
            var refreshTokenProtector =
                bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var refreshTicket = refreshTokenProtector.Unprotect(request.RefreshToken);

            // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                timeProvider.GetUtcNow() >= expiresUtc ||
                await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)

            {
                return TypedResults.Challenge();
            }

            var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            
            return Results.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        });

    app.Run();
}

public record RefreshTokenRequest(string RefreshToken);