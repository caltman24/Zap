using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Scalar.AspNetCore;
using Zap.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddResponseCaching();


builder.Services.AddDataAccess(builder.Configuration)
    .AddIdentityManagement()
    .AddAuthService()
    .AddCorsPolicies();

builder.Services.AddAWSService<IAmazonS3>(new AWSOptions()
{
    Region = Amazon.RegionEndpoint.USEast1,
    Credentials = new Amazon.Runtime.BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"]),
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapZapApiEndpoints();

app.Run();