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

Make small, reviewable changes only.

Follow these constraints strictly:

1. LIMIT FILE CHANGES
Do not modify more than 5–8 files in a single implementation step.

If a change requires more files, split the work into multiple steps and stop after the first step.

2. ONE CONCERN PER STEP
Each implementation step should solve only one concern.

Examples:
- update server authorization
- update database query filtering
- update API endpoint behavior
- update client UI
- add tests

Do not combine multiple concerns in a single change.

3. DO NOT TOUCH UNRELATED CODE
Only modify files directly required for the change.
Do not refactor, rename, or reorganize unrelated files.

4. FOLLOW EXISTING ARCHITECTURE
Reuse existing patterns, services, helpers, and structure.
Do not introduce new patterns unless explicitly instructed.

5. AVOID DUPLICATED LOGIC
Before adding new logic, search the repository for similar functionality and reuse existing code when possible.

6. NO LARGE REFACTORING
Do not perform sweeping refactors while implementing features.
If refactoring is needed, propose it separately.

7. STOP AFTER IMPLEMENTATION
After completing the implementation step, stop and summarize:
- files changed
- what logic was introduced
- potential follow-up steps

Do not automatically proceed to additional steps unless asked.

Your priority is safe, incremental changes that are easy to review.
