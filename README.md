# Zap

Zap is a project management and bug tracking application (think simplified Jira/Linear). It helps companies manage projects, assign team members, track tickets (bugs, features, tasks), and collaborate via comments and file attachments.

---

## Architecture

A simplified architecture diagram:

```text
Browser
  |
  v
Remix SSR (Netlify)
  -- HTTP/JSON -->
.NET 9 Minimal API (Railway)
  |
  +--> PostgreSQL
  +--> AWS S3 (attachments)
```

Notes: the client is a Remix v2 + Vite app that runs server-side. All API calls are proxied through Remix loaders/actions — the browser never talks directly to the .NET backend. The backend is an ASP.NET Core 9 Minimal API using a vertical-slice architecture.

---

## Project Structure

zap/
  AGENTS.md          # Agent coding guide
  todo.txt           # Project task list
  client/            # Remix + Vite TypeScript frontend
  server/            # .NET 9 API backend

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
- `client/app/data/permissions.ts` — permission matrix
- `client/app/data/routes.ts` — sidebar/navigation structure

Key server files and locations:
- `server/Zap.Api/Program.cs` — entry point and service registration
- `server/Zap.Api/Configuration/ConfigureServices.cs` — DI (DB, auth, S3, rate limiting)
- `server/Zap.Api/Configuration/Endpoints.cs` — endpoint registration
- `server/Features/` — vertical-slice feature folders (Authentication, Companies, Projects, Tickets, FileUpload)
- `server/Data/AppDbContext.cs` — EF Core DbContext (PostgreSQL)
- `server/Zap.Tests/` — xUnit integration tests (in-memory DB)

---

## Tech Stack

- Client: React 18, Remix 2, Vite 6, TypeScript 5, Tailwind CSS 4, DaisyUI 5, pnpm
- Server: .NET 9, ASP.NET Core Minimal API, EF Core 9 (Npgsql/PostgreSQL), ASP.NET Identity (Bearer tokens)
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
2. Fetcher-based mutations (client) — headless routes like `/tickets/:id/update-priority` enable optimistic UI without full navigation
3. Shared route logic — `client/app/routes/commonRoutes/projectDetails` is reused across views (all/my/archived)
4. Role-based access — enforced via authorization policies, endpoint filters, and service validation
5. Token auth flow — login returns bearer + refresh tokens stored in session cookie; client auto-refreshes with a 2-minute expiry buffer
6. Ticket history — all ticket mutations create audit trail entries with human-friendly messages

---

## Key Routes / Features

| Feature | URL | Roles |
|---------|-----|-------|
| Dashboard | `/dashboard` | All |
| Company Details | `/company` | All (edit: Admin) |
| All Projects | `/projects` | All |
| My Projects | `/projects/myprojects` | PM, Dev, Submitter |
| Create Project | `/projects/new` | Admin, PM |
| Project Detail | `/projects/:id` | All |
| Ticket Detail | `/projects/:id/tickets/:ticketId` | All (restrictions apply) |
| All Tickets | `/tickets` | All |
| Create Ticket | `/tickets/new` | Admin, PM, Submitter |

---

## Development Setup

Prerequisites
- Node 20+ (client)
- pnpm (preferred package manager)
- .NET 9 SDK (server)
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
- Server integration tests use in-memory DB by default; full end-to-end requires a real Postgres instance and any EF migrations to be applied.

Environment variables
- The API loads configuration from `.env` located in `server/Zap.Api` during local development. Typical variables:
  - `ASPNETCORE_ENVIRONMENT`
  - `ConnectionStrings__Default` (Postgres connection)
  - AWS credentials (e.g. `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`, `AWS_S3_BUCKET`)
  - JWT signing/issuer related secrets

Applying EF migrations
- If you change data models you may need to apply EF migrations from `server/` (use `dotnet ef` or standard migration workflow in the solution). Check `server/Zap.Api` for migration guidance.

---

## Notable Gaps / In-Progress Items

- No CI/CD pipeline (no GitHub Actions or similar configured)
- No client tests configured (no test runner present)
- Incomplete features (see `todo.txt`): user settings/profile, admin tools, invite system, realtime notifications
- Stubs / TODOs in code: `DeleteCompany` throws `NotImplementedException`, `TicketAttachmentsService` is empty

---

## Contributing

1. Follow the conventions in `AGENTS.md` for edits and new features.
2. Keep changes small and focused; run `pnpm lint` and `pnpm typecheck` for client changes and `dotnet test` for server tests.
3. When introducing new packages to the client, use `pnpm` and document any new scripts in `client/package.json`.

---

## License

This repository does not include an explicit license file. Add a `LICENSE` file if you intend to make the project open source.

---

If anything in this README needs more detail (example env values, migration commands, or startup troubleshooting), tell me which area to expand and I will update `README.md`.
