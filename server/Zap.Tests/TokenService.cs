﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;
using Zap.Api.Common.Constants;
using Zap.Api.Data.Models;

namespace Zap.Tests;

public sealed class TokenService(SignInManager<AppUser> signInManager, IOptionsMonitor<BearerTokenOptions> options)
{
    private readonly BearerTokenOptions _options = options.Get(IdentityConstants.BearerScheme);

    public async Task<string> GenerateTokenAsync(string username, string role = RoleNames.Admin)
    {
        var claimsPrincipal =
            await signInManager.CreateUserPrincipalAsync(new AppUser
                { Id = username, UserName = username, FirstName = "Test", LastName = "User" });

        ((ClaimsIdentity?)claimsPrincipal.Identity)?.AddClaim(new("role", role));

        // This is copied from https://github.com/dotnet/aspnetcore/blob/238dabc8bf7a6d9485d420db01d7942044b218ee/src/Security/Authentication/BearerToken/src/BearerTokenHandler.cs#L66
        var timeProvider = _options.TimeProvider ?? TimeProvider.System;

        var utcNow = timeProvider.GetUtcNow();

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow + _options.BearerTokenExpiration
        };

        var ticket = new AuthenticationTicket(
            claimsPrincipal, properties, $"{IdentityConstants.BearerScheme}:AccessToken");

        return _options.BearerTokenProtector.Protect(ticket);
    }
}