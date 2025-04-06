using dotenv.net;
using FluentValidation;
using Scalar.AspNetCore;
using Zap.Api;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Extensions;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddStructuredLogging(builder.Configuration);
builder.Services.AddRequiredServices(builder.Configuration);


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseRequiredServices();
app.MapZapApiEndpoints();

app.Run();

public partial class Program
{
}