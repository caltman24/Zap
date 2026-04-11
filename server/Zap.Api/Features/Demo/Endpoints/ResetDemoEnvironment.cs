using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Features.Demo.Services;

namespace Zap.Api.Features.Demo.Endpoints;

public class ResetDemoEnvironment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/reset", Handle);
    }

    private static async Task<Results<ForbidHttpResult, NoContent>> Handle(
        HttpRequest request,
        IConfiguration configuration,
        IDemoEnvironmentService demoEnvironmentService)
    {
        var isEnabled = configuration["Demo:EnableReset"];
        var expectedKey = configuration["Demo:ResetKey"];
        var providedKey = request.Headers["X-Demo-Reset-Key"].ToString();

        if (!string.Equals(isEnabled, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(expectedKey) ||
            expectedKey != providedKey)
            return TypedResults.Forbid();

        await demoEnvironmentService.ResetDemoEnvironmentAsync();
        return TypedResults.NoContent();
    }
}