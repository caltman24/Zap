using dotenv.net;
using Scalar.AspNetCore;
using Zap.Api;

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

namespace Zap.Api
{
    public partial class Program
    {
    }
}