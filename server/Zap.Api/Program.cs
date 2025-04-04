using dotenv.net;
using FluentValidation;
using Scalar.AspNetCore;
using Zap.Api.Authorization;
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
    .AddS3Storage()
    .AddCurrentUser()
    .AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());

app.UseRequiredServices();
app.MapZapApiEndpoints();

app.Run();

public partial class Program { }