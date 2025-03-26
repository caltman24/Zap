using System.Threading.RateLimiting;
using dotenv.net;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using Zap.Api.Extensions;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddRateLimiting()
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

app.UseRequiredServices();
app.MapZapApiEndpoints();

app.Run();