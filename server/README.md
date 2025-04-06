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

### Database Seeding
`appsettings.<env>.json` has a `UseSeeding` option to seed the database with roles.
