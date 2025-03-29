using dotenv.net;
using Scalar.AspNetCore;
using Zap.Api.Extensions;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddStructuredLogging(builder.Configuration);

builder.Services.AddOpenApi()
    .AddRateLimiting()
    .AddDataAccess(builder.Configuration)
    .AddIdentityManagement()
    .AddAuthService()
    .AddCorsPolicies()
    .AddS3Storage();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());

app.UseRequiredServices();
app.MapZapApiEndpoints();

app.Run();