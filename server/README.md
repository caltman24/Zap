# Zap Server

- .NET 9
- Vertical Slice Architecture
- Minimal API
- PostgreSQL
- Bearer Token Auth
- Role Based Authorization
- Identity Framework
- Entity Framework
- AWS S3


## Projects

- ### API
  - Uses Postgres in Dev/Prod
- ### Tests
  - Integration Tests with In Memory DB

    
## Setup

### Restore Dependencies
```term
dotnet restore
```
### Migrations
- Add ```appsettings.development.json``` to project with ```DefaultConnection``` Connection String
- Execute Migrations: ```dotnet ef database update```
- Add Migrations: ```dotnet ef migrations add <name> -o ./Data/Migrations```

### Automatic migrations at startup (APPLY_MIGRATIONS)

- The API can apply EF Core migrations automatically on startup when the environment variable
  `APPLY_MIGRATIONS` is set to `true`. This is useful for first-run / bootstrap deployments.
- Recommended pattern for a first-run deployment:

  1) Deploy with migrations enabled:

     ```sh
     docker run --rm -p 5090:80 \
       -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=zap_dev;Username=postgres;Password=postgres" \
       -e APPLY_MIGRATIONS=true \
       zap-api:local
     ```

  2) Confirm the app started and migrations applied (check logs or `GET /health`).
  3) For production safety, unset or set `APPLY_MIGRATIONS=false` after initial migration so
     schema changes are controlled via your release process.

Notes:
- By default the app will apply migrations automatically in `Development` and `Testing` environments.
- In Production the default is to skip automatic migrations unless `APPLY_MIGRATIONS=true` is explicitly set.
### Docker / Local Run

- Build image from repo root:
  `docker build -t zap-api:local server`
- Run container and provide Postgres connection string via env var:
  `docker run --rm -p 5090:80 -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=zap_dev;Username=postgres;Password=postgres" zap-api:local`

Notes:
- The app exposes a `/health` endpoint at port 80 for smoke checks.
- On startup the application will attempt to apply any pending EF Core migrations.

### Database Seeding
When using `EnsureCreated` or `ef database update`, roles will automatically be seeded if they do not already exist.
