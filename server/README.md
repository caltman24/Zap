# Zap Server

- .NET 9
- Minimal API
- PostgreSQL
- Bearer Token Auth
- Role Based Authorization
- Identity Framework
- Entity Framework


## Projects

- ### API

- ### DataAccess
  - Holds the AppDbContext and Migrations
  - Executing Assembly for running migrations
  - ```DesignTimeDbContextFactory.cs```  creates a derived DbContext for this assembly
    
## Setup

### Restore Dependencies
```term
dotnet restore
```
### Exectute Migrations
Add ```appsettings.development.json``` to DataAccess project with ```DefaultConnection``` Connection String
```term
Zap.DataAccess/ > dotnet ef database update
```
