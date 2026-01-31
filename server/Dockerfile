# Multi-stage Dockerfile for ASP.NET Core 9 API
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ./Zap.Api/*.csproj ./Zap.Api/
COPY ./Zap.DataAccess/*.csproj ./Zap.DataAccess/
COPY ./Zap.Tests/*.csproj ./Zap.Tests/
RUN dotnet restore ./Zap.Api/Zap.Api.csproj

# copy everything and publish
COPY . .
WORKDIR /src/Zap.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "Zap.Api.dll"]
