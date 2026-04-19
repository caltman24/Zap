# Zap

Zap is a full-stack Jira/Linear-style project management app I built as a hiring project.

Its job is simple: prove I can design and ship a non-trivial product with real authorization rules, server-enforced data
scoping, clean full-stack boundaries, and maintainable architecture.

Companies can manage projects, assign members, track tickets, and collaborate through comments and attachments.

## Highlights

- Server-first authorization: the API decides what each role can see and do, and the UI mirrors that behavior through
  API-driven permissions and resource capabilities
- Scoped data access: developers and submitters only receive projects and tickets they are actually allowed to access
- Full-stack architecture: Remix SSR frontend, ASP.NET Core Minimal API backend, PostgreSQL persistence, S3-ready
  attachment path
- Maintainable backend design: vertical slices, thin endpoints, focused services, and a mix of integration and unit
  tests where it matters
- Product-minded implementation: role-aware navigation, route gating, ticket workflows, comments, project views, and
  audit history

---

## Architecture

A simplified architecture diagram:

```text
Browser
  |
  v
Remix SSR (Netlify)
  -- HTTP/JSON -->
.NET 10 Minimal API (Railway)
  |
  +--> PostgreSQL
  +--> AWS S3 (attachments)
```

The client is a Remix v2 + Vite app that runs server-side. Remix handles the API calls through loaders/actions, so the
browser never talks directly to the .NET backend. The backend is an ASP.NET Core 10 Minimal API built with a
vertical-slice structure.

---

## What This Demonstrates

- Full-stack product development with a Remix SSR frontend and ASP.NET Core Minimal API backend
- API-driven authorization and UI gating: global permissions come from `/user/info`, while project and ticket actions
  come from resource capabilities
- Vertical slice backend architecture with thin endpoints and service-based business logic
- Session-based auth in the web app with token refresh handling
- Scoped data access so users only receive the projects and tickets relevant to their role
- A testing strategy that uses integration tests for end-to-end behavior and unit tests for rule-heavy services,
  validators, and audit/history logic

---

## Why I Built This

I built Zap to be a strong hiring project rather than a tutorial app. The goal was to show that I can build software
with:

- clear domain modeling
- real authorization rules instead of superficial role labels
- server-enforced data scoping
- a clean SSR client/backend boundary
- maintainable patterns that can grow over time

If you are reviewing this as part of an interview or application, the best places to look are:

- `client/app/data/routes.ts` and `client/app/utils/permissions.ts`
- `client/app/commonRoutes/projectDetails/commonRoute.tsx`
- `client/app/routes/_app.projects.$projectId.tickets.$ticketId/route.tsx`
- `server/Zap.Api/Features/Tickets/Services/TicketAuthorizationService.cs`
- `server/Zap.Api/Features/Projects/Services/ProjectAuthorizationService.cs`
- `server/Zap.Api/Features/Users/Services/UserPermissionService.cs`
- `server/Zap.Tests/Features/`
- `server/Zap.Tests/Unit/`

---

## Project Structure

zap/
AGENTS.md # Agent coding guide
todo.txt # Project task list
client/ # Remix + Vite TypeScript frontend
server/ # .NET 10 API backend

Key client files and locations:

- `client/app/routes/_landing.*` — public pages (home, login, register)
- `client/app/routes/_setup.*` — post-registration company setup
- `client/app/routes/_app.*` — authenticated app (dashboard, projects, tickets)
- `client/app/services/api.server/apiClient.ts` — singleton API client
- `client/app/services/api.server/authClient.ts` — auth endpoints (sign-in, register, refresh)
- `client/app/services/sessions.server.ts` — cookie-based session management
- `client/app/components/TicketTable.tsx` — ticket listing table
- `client/app/components/ProjectCard.tsx` — project card for grids
- `client/app/components/EditModeForm.tsx` — reusable edit wrapper
- `client/app/data/roles.ts` — app roles
- `client/app/data/routes.ts` — sidebar/navigation structure and required global permissions
- `client/app/utils/permissions.ts` — global permission helper
- `client/app/utils/commentPermissions.ts` — temporary comment UI permission helper
- `client/app/commonRoutes/projectDetails/commonRoute.tsx` — project detail UI driven by API capabilities

Key server files and locations:

- `server/Zap.Api/Program.cs` — entry point and service registration
- `server/Zap.Api/Configuration/ConfigureServices.cs` — DI (DB, auth, S3, rate limiting)
- `server/Zap.Api/Configuration/Endpoints.cs` — endpoint registration
- `server/Features/` — vertical-slice feature folders (Authentication, Companies, Projects, Tickets, FileUpload)
- `server/Data/AppDbContext.cs` — EF Core DbContext (PostgreSQL)
- `server/Zap.Tests/Features/` — xUnit integration tests for endpoint behavior and authorization flows
- `server/Zap.Tests/Unit/` — focused unit tests for validators, permission rules, capability logic, and selected
  service behavior

---

## Tech Stack

- Client: React 18, Remix 2, Vite 6, TypeScript 5, Tailwind CSS 4, DaisyUI 5, pnpm
- Server: .NET 10, ASP.NET Core Minimal API, EF Core 10 (Npgsql/PostgreSQL), ASP.NET Identity (Bearer tokens)
- Infrastructure: PostgreSQL, AWS S3 (for attachments), Serilog, Bogus (test data)

---

## Core Domain Model

- Company — owns members and projects
- CompanyMember — a user within a company with a role
- CompanyRole — Admin, Project Manager, Developer, Submitter
- Project — belongs to a company, has a PM, assigned members, and tickets
- Ticket — belongs to a project, has priority/status/type, submitter, optional assignee
- TicketComment / TicketAttachment / TicketHistory — ticket collaboration data

---

## Key Patterns

1. Vertical slice architecture (server) — each feature is self-contained with endpoints, services, DTOs, and filters
2. Fetcher-based mutations (client) — headless routes like `/tickets/:id/update-priority` enable optimistic UI without
   full navigation
3. Shared route logic — `client/app/routes/commonRoutes/projectDetails` is reused across views (all/my/archived)
4. API-driven access model — global app permissions come from `/user/info`, while project/ticket action visibility comes
   from resource `capabilities`
5. Token auth flow — login returns bearer + refresh tokens stored in session cookie; client auto-refreshes with a
   2-minute expiry buffer
6. Ticket history — all ticket mutations create audit trail entries with human-friendly messages

---

## Testing Strategy

The backend tests are split pretty deliberately.

- Integration tests under `server/Zap.Tests/Features/` cover the stuff that really needs the full stack: routing,
  auth, endpoint filters, data scoping, persistence, and role-sensitive workflows
- Unit tests under `server/Zap.Tests/Unit/` cover the logic that benefits from fast, isolated feedback: validators,
  permission maps, capability calculations, comment rules, and ticket history side effects

I do not try to unit test every service just because it exists. I use unit tests where the logic itself is the point,
and integration tests where the wiring and persistence are the point.

Examples of logic covered by unit tests:

- `TicketAuthorizationRules` and `UserPermissionService` for role-based rule matrices
- FluentValidation request validators for endpoint contracts
- `TicketAuthorizationService` and `ProjectAuthorizationService` for capability and access decisions
- selected `TicketService`, `TicketCommentsService`, `ProjectService`, and `CompanyService` methods where branching or
  history/file side effects matter

Things I keep integration-first:

- minimal API handlers and endpoint filters
- auth/current-user wiring
- scoped query behavior through EF and PostgreSQL
- full request/response flows for projects, tickets, comments, and company actions

---

## Role Matrix

The API is the source of truth for permissions. The Remix client mirrors those rules by hiding routes, buttons, and
actions that do not apply.

- Global permissions are returned from `/user/info` as `user.permissions` and drive navigation, page access, and broad
  UI actions like `company.edit`, `project.create`, and `project.assignPm`
- Resource-specific actions are returned on project and ticket payloads as `capabilities`, so UI decisions stay aligned
  with live server state such as project membership, PM ownership, ticket assignee, archive status, and ticket status
- The API still enforces everything server-side; the client uses permissions/capabilities as presentation hints rather
  than the final authority

### Admin

- Full company visibility and management
- Can view all company projects and tickets
- Can create, edit, archive, and delete projects and tickets
- Can assign project managers, manage project membership, and manage ticket workflow fields
- Can view and edit company details

### Project Manager

- Scoped to projects they manage
- Can view managed projects and tickets in those projects
- Can create, edit, archive, and delete tickets in managed projects
- Can change ticket status, priority, type, and assignee in managed projects
- Can manage membership and details for managed projects

### Developer

- Scoped to projects they are assigned to
- Can view assigned projects and tickets in those projects
- Can view tickets directly assigned to them through `My Tickets`
- Can comment on visible tickets
- Can update ticket status only when directly assigned to that ticket
- Cannot manage projects, assign tickets, or change priority/type/archive/delete ticket state

### Submitter

- Scoped to projects they are assigned to
- Can view assigned projects and tickets in those projects
- Can create tickets in assigned projects
- Can comment on visible tickets
- Can edit their own ticket title and description only while the ticket is `New`
- Cannot change status, priority, type, assignee, archive, or delete ticket state

This is intentionally server-first: the API scopes the data and permissions, and the client follows by hiding the
navigation, routes, and controls that do not make sense for the current user.

---

## Key Routes / Features

| Feature           | URL                               | Roles                    |
|-------------------|-----------------------------------|--------------------------|
| Dashboard         | `/dashboard`                      | All                      |
| Company Details   | `/company`                        | Admin                    |
| All Projects      | `/projects`                       | Admin, PM                |
| My Projects       | `/projects/myprojects`            | PM, Dev, Submitter       |
| Create Project    | `/projects/new`                   | Admin, PM                |
| Archived Projects | `/projects/archived`              | Admin, PM                |
| Project Detail    | `/projects/:id`                   | Scoped by project access |
| Ticket Detail     | `/projects/:id/tickets/:ticketId` | Scoped by ticket access  |
| All Tickets       | `/tickets`                        | All                      |
| My Tickets        | `/tickets/mytickets`              | All                      |
| Resolved Tickets  | `/tickets/resolved`               | All                      |
| Archived Tickets  | `/tickets/archived`               | All                      |
| Create Ticket     | `/tickets/new`                    | Admin, PM, Submitter     |

---

## Dashboard Search

The dashboard navbar includes a live search for quickly jumping to tickets and projects.

- Search is still role-aware and server-enforced, so users only see active tickets and projects they can already access
- To keep the results focused, the search only looks at:
    - `ticket.displayId`
    - `ticket.name`
    - `project.name`
- Ticket `displayId` matches are given the highest priority and support:
    - exact search, like `#ZAP-47FD`
    - prefix search, like `zap-47`
    - partial search, like `47`
- Ticket and project names use case-insensitive prefix-of-any-word matching, so both `ope` and `repor` can match
  `Operations Reporting`
- Results are labeled by type (`Ticket` or `Project`) and take you straight to the matching detail page

Results are ranked by usefulness rather than alphabetically. Under the hood, each match gets a score and the highest
score wins. The scoring is intentionally biased toward ticket IDs because they are the most specific search input a user
can type.

- exact ticket `displayId`
- prefix ticket `displayId`
- partial ticket `displayId`
- ticket name word-prefix matches
- project name word-prefix matches

In practice, that means:

- typing an exact ticket ID like `#ZAP-47FD` will beat any name match
- typing the start of a ticket ID like `zap-47` still ranks above ticket or project names
- partial ticket ID matches like `47` still outrank name matches
- ticket name matches are ranked above project name matches
- if two results have the same score, they fall back to alphabetical order

The goal is to make ID lookups feel fast and precise while still making the search bar useful for everyday navigation.

---

## Development Setup

Prerequisites

- Node 20+ (client)
- pnpm (preferred package manager)
- .NET 10 SDK (server)
- PostgreSQL running (default assumed at `localhost:5432`)
- AWS S3 credentials for file uploads (used by `server/Features/FileUpload`)

Common commands

- Client dev: `cd client && pnpm install && pnpm dev` (default port: 5173)
- Server dev: `cd server/Zap.Api && dotnet restore && dotnet run` (default port: 5090)
- Server tests: `cd server/Zap.Tests && dotnet test`
- Docker (Full Stack): `docker-compose up` (starts PostgreSQL and API)
- Client lint: `cd client && pnpm lint`
- Client typecheck: `cd client && pnpm typecheck`

Notes

- Use `pnpm` for the client — do not add `package-lock.json` or `yarn.lock`.
- The server test project contains both integration tests (`Features/`) and unit tests (`Unit/`).
- Most integration tests use an in-memory DB by default, while a few provider-sensitive cases use the real test
  database path.
- Full end-to-end verification still requires a real Postgres instance and any EF migrations to be applied.

Environment variables

- The API loads configuration from `.env` located in `server/Zap.Api` during local development. Typical variables:
    - `ASPNETCORE_ENVIRONMENT`
    - `ConnectionStrings__Default` (Postgres connection)
    - Local AWS auth selection (typically `AWS_PROFILE`)
    - Store local AWS credentials outside the repo in `~/.aws/credentials` and `~/.aws/config`
    - S3 bucket/region live in committed `appsettings*.json`, with `Development` and `Testing` using different buckets
    - Real S3 integration tests use the `Testing` environment and the same local AWS profile setup
    - JWT signing/issuer related secrets

Applying EF migrations

- If you change data models you may need to apply EF migrations from `server/` (use `dotnet ef` or standard migration
  workflow in the solution). Check `server/Zap.Api` for migration guidance.

---

## Notable Gaps / In-Progress Items

- No CI/CD pipeline (no GitHub Actions or similar configured)
- No client tests configured (no test runner present)
- Incomplete features (see `todo.txt`): user settings/profile, admin tools, invite system, realtime notifications
