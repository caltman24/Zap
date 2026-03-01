# AGENTS

Use this file as project-specific implementation context.

## Architecture

- Remix app (`client/`):
  - Browser should not call the .NET API directly. Use Remix loaders/actions and the shared client in `client/app/services/api.server/apiClient.ts`.
  - Auth state lives in cookie session (`client/app/services/sessions.server.ts`) with `tokens` and `user`. In protected loaders/actions, call `apiClient.auth.getValidToken(session)` and forward returned `Set-Cookie` headers when present.
  - Use `client/app/data/permissions.ts` with `validateRole` for role checks instead of ad-hoc role logic.
  - Reuse shared route logic when available (for example `client/app/commonRoutes/projectDetails/*`) instead of duplicating behavior across route variants.
- .NET API (`server/Zap.Api`):
  - Follow Vertical Slice Architecture: place feature code under `Features/<Feature>/{Endpoints,Services,Filters}` and keep endpoint handlers thin.
  - Endpoints follow the `IEndpoint` pattern (static `Map`) and must be registered in `Configuration/Endpoints.cs`.
  - Enforce tenant/role access with existing extensions and filters (`WithCompanyMember`, `WithProjectCompanyValidation`, `WithTicketCompanyValidation`) rather than custom ad-hoc checks.
  - Prefer FluentValidation request validators with `.WithRequestValidation<TRequest>()`.

## Working Rules

- Prefer existing patterns over introducing new abstractions.
- Keep changes aligned with the current system design.
