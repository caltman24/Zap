# Railway deploy checklist

Required environment variables:

- ConnectionStrings__DefaultConnection : PostgreSQL connection string
  - Example: Host=container_db;Port=5432;Database=zap_prod;Username=postgres;Password=postgres
- AWS_REGION (optional if using S3)
- AWS_ACCESS_KEY (optional if using S3)
- AWS_SECRET_KEY (optional if using S3)
- AWS_S3_BUCKET (optional if using S3)
- DOTNET_ENVIRONMENT : set to "Production"
- ASPNETCORE_URLS : typically not required; Dockerfile sets to http://+:80

Notes:
- Railway should expose the container port 80 and map it to an external port.
- Ensure the database is accessible to the service; use Railway Postgres add-on and set
  ConnectionStrings__DefaultConnection accordingly.
- EF Core migrations are applied on startup only when `APPLY_MIGRATIONS=true` (or by default in Development/Testing). In Production, set this variable or run migrations manually, and monitor logs for migration output.
