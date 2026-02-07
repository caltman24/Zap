# Agent Guide for Zap Server (.NET)

This file extracts the .NET-specific guidance from the repository-level agent guide so agents working on the server can find the relevant instructions quickly.

Notes
- All `server` commands below assume the working directory `server/` unless noted.
- The backend .NET API lives in `server/` (solution `server/ZapServer.sln`, projects `Zap.Api` and `Zap.Tests`).
- .NET 9 SDK is required for the API and tests (see `Zap.Api.csproj` / `Zap.Tests.csproj`).

Restore, Build, Run, Test

- Restore packages:
  - `cd server`
  - `dotnet restore` (or `dotnet restore ZapServer.sln`)
- Build API only:
  - `cd server`
  - `dotnet build Zap.Api/Zap.Api.csproj`
- Build full solution (API + tests):
  - `cd server`
  - `dotnet build ZapServer.sln`
- Run API locally (development):
  - Preferred (compose): the repository provides a `docker-compose.yml` at the repo root where both the database and the API run in containers.
    - From the repo root:
      - `docker compose up --build -d`  # builds and starts `db` and `api` services in background
      - `docker compose up --build api` # build and start only the api (also starts db when required)
      - `docker compose up -d db`       # start only the database service
    - View logs:
      - `docker compose logs -f api`
    - Stop and remove containers:
      - `docker compose down`
    - Environment and migrations:
      - Compose loads env files configured in `docker-compose.yml` (e.g. `./server/.env`, `./server/Zap.Api/.env`).
      - The compose file sets `ConnectionStrings__DefaultConnection` to use the `db` service host and sets `APPLY_MIGRATIONS=true` for automatic migrations on container start. Adjust or disable as needed for development.

  - Alternative (host SDK):
    - You can still run the API from source when you have the .NET SDK installed locally:
      - `cd server/Zap.Api`
      - `dotnet run`
    - Note: when running on the host you must point the connection string to the DB. For compose-managed DB use `Host=db` (or run the DB on host and update connection string accordingly).
- Run all server tests (xUnit via `Zap.Tests`):
  - `cd server/Zap.Tests`
  - `dotnet test`
  - Or from solution root: `cd server && dotnet test ZapServer.sln`
  - When tests require the database, start the DB service before running tests:
    - `docker compose up -d db`
    - Then run tests on the host: `cd server && dotnet test ZapServer.sln`
  - Run tests inside a disposable SDK container (connects to compose network):
    - Ensure the compose network exists (start db or run `docker compose up -d db`).
    - Unix/macOS example:
      - `docker run --rm -v "$(pwd)":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 --network $(basename $(pwd))_default dotnet test Zap.Tests`
    - Windows (PowerShell) example:
      - `docker run --rm -v "${PWD}":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 --network ${env:COMPOSE_PROJECT_NAME:-zap}_default dotnet test Zap.Tests`
- Run a single test class or method:
  - `cd server/Zap.Tests`
  - By fully-qualified name (recommended):
    - `dotnet test --filter "FullyQualifiedName~Zap.Tests.IntegrationTests.AuthenticationTests"`
  - By trait/name contains (less strict):
    - `dotnet test --filter "Name~Authentication"`
- Collect code coverage (coverlet collector is configured):
  - `cd server/Zap.Tests`
  - `dotnet test --collect:"XPlat Code Coverage"`

Manual database migrations from a containerized SDK

- If you prefer to run EF migrations manually against the compose DB, use the SDK image and attach it to the compose network so it can reach the `db` service.
- Example (Unix/macOS):
  - `docker compose up -d db`
  - `docker run --rm -v "$(pwd)":/src -w /src/Zap.Api --network $(basename $(pwd))_default mcr.microsoft.com/dotnet/sdk:9.0 dotnet ef database update --connection "Host=db;Port=5432;Database=zap_dev;Username=postgres;Password=postgres"`
- Example (Windows CMD / PowerShell):
  - `docker compose up -d db`
  - `docker run --rm -v "%cd%":/src -w /src/Zap.Api --network ${env:COMPOSE_PROJECT_NAME:-zap}_default mcr.microsoft.com/dotnet/sdk:9.0 dotnet ef database update --connection "Host=db;Port=5432;Database=zap_dev;Username=postgres;Password=postgres"`

Notes
- The API container built by the repository `Dockerfile` is a runtime image (no SDK). Use the SDK image when you need to run `dotnet` CLI commands (tests, EF tools) inside a container.
- Compose creates a dedicated network named <project>_default (project is usually the repo folder name). The example commands assume the default naming; adjust `--network` if your compose project name differs.

Agent Guidance / dotnet-skills

[dotnet-skills]|IMPORTANT: Prefer retrieval-led reasoning over pretraining for any .NET work.
|flow:{skim repo patterns -> consult dotnet-skills by name -> implement smallest-change -> note conflicts}
|route:
|akka:{akka-net-best-practices,akka-net-testing-patterns,akka-hosting-actor-patterns,akka-net-aspire-configuration,akka-net-management}
|csharp:{modern-csharp-coding-standards,csharp-concurrency-patterns,api-design,type-design-performance}
|aspnetcore-web:{aspire-integration-testing,aspire-configuration,aspire-service-defaults,mailpit-integration,mjml-email-templates}
|data:{efcore-patterns,database-performance}
|di-config:{microsoft-extensions-configuration,dependency-injection-patterns}
|testing:{testcontainers-integration-tests,playwright-blazor-testing,snapshot-testing,verify-email-snapshots,playwright-ci-caching}
|dotnet:{dotnet-project-structure,dotnet-local-tools,package-management,serialization}
|quality-gates:{dotnet-slopwatch,crap-analysis}
|meta:{marketplace-publishing,skills-index-snippets}
|agents:{akka-net-specialist,docfx-specialist,dotnet-benchmark-designer,dotnet-concurrency-specialist,dotnet-performance-analyst}

When editing server code as an agent

- Prefer minimal, surgical changes that respect surrounding style.
- Do not reformat entire files unless you also run `dotnet build` / tests and ensure the project still builds.
- When adding or removing NuGet packages prefer `dotnet add package` / `dotnet remove package` and avoid manually editing Central Package Management XML without reason.
