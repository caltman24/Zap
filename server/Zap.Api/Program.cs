using dotenv.net;
using dotenv.net.Utilities;
using Scalar.AspNetCore;
using Zap.Api;
using Zap.Api.Configuration;


var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();

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