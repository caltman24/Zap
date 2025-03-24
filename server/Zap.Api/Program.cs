using Scalar.AspNetCore;
using Zap.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDataAccess(builder.Configuration)
    .AddIdentityManagement()
    .AddAuthService()
    .AddCorsPolicies();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapZapApiEndpoints();

app.Run();