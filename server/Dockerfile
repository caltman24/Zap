# Multi-stage Dockerfile for ASP.NET Core 9 API
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ./Zap.Api/*.csproj ./Zap.Api/
COPY ./Zap.DataAccess/*.csproj ./Zap.DataAccess/
COPY ./Zap.Tests/*.csproj ./Zap.Tests/
# Restore explicitly from nuget.org inside the build container to avoid relying on
# any host-side NuGet configuration or cached feeds that are not available in the
# clean container environment.
RUN dotnet restore ./Zap.Api/Zap.Api.csproj --source https://api.nuget.org/v3/index.json

# copy everything and publish
COPY . .
WORKDIR /src/Zap.Api
# Allow publish to perform its own restore if the previous restore layer was incomplete
# (removes the --no-restore flag which caused publish to fail when restore wasn't complete).
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "Zap.Api.dll"]
