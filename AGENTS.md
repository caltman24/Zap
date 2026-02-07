# Agent Guide for Zap Repository

This document describes how agentic coding tools should work in this repo: how to build, lint, and (where available) test the project, and how to follow local coding conventions.

## 1. Project Layout

- Root repo contains a Remix + Vite TypeScript client in `client/`.
- Backend .NET API lives in `server/` (solution `server/ZapServer.sln`, projects `Zap.Api` and `Zap.Tests`). See `server/AGENT.md` for server-specific agent guidance (build, run, compose, tests, migrations).
- Server agent guide (quick link): `server/AGENT.md`
- All `client` commands below assume the working directory `client/` unless noted.
- All `server` commands below assume the working directory `server/` unless noted.
- Node 20+ is required for the client (see `client/package.json` `engines.node`).
- .NET 9 SDK is required for the API and tests (see `Zap.Api.csproj` / `Zap.Tests.csproj`).

## 2. Installation & Package Manager

- Preferred package manager: `pnpm` (inferred from `package.json.pnpm` block).
- Install dependencies:
  - `cd client`
  - `pnpm install`
- Avoid switching package managers; do not add `package-lock.json` or `yarn.lock`.

## 3. Build, Dev, Lint, Typecheck, Test

### 3.1 Client (Remix + Vite)

- Dev server:
  - `cd client`
  - `pnpm dev`
- Production build:
  - `cd client`
  - `pnpm build`
- Start built app:
  - `cd client`
  - `pnpm start`
- Lint all files (ESLint + TypeScript plugin):
  - `cd client`
  - `pnpm lint`
  - Uses `.gitignore` as the ignore list and an on-disk ESLint cache.
- Type-check only (no emit):
  - `cd client`
  - `pnpm typecheck`
- Client tests:
  - No explicit test script is defined in `client/package.json` and no obvious test runner config is present.
  - If you generate tests, prefer using the same runner across the codebase and add a `test` script instead of ad-hoc commands.
  - For now, there is **no canonical client single-test command**; if you introduce one, document it here and keep it consistent.



## 4. Running a Single Check During Development

- To quickly validate changes around types only:
  - `cd client && pnpm typecheck`
- To validate style and imports only:
  - `cd client && pnpm lint path/to/file.tsx`
  - ESLint accepts file or glob arguments, so you can target a single file or small set when iterating.

## 5. TypeScript & General Code Style

- Language:
  - Use TypeScript (`.ts` / `.tsx`) for all new code in `client/app`.
  - Use modern ES modules (`import` / `export`); the project has `"type": "module"` in `client/package.json`.
- Strictness:
  - Respect the existing `tsconfig` (not shown here); do not relax compiler options without explicit direction.
  - Prefer explicit return types for exported functions, loaders, actions, and React components.
- Types vs interfaces:
  - Either `type` or `interface` is acceptable; follow the surrounding file.
  - For data shapes from the API layer, keep definitions in `app/services/api.server/types.ts` or adjacent `types.ts` files.
- Nullability:
  - Model optional and nullable fields explicitly (`foo?: string` or `foo: string | null`).
  - Avoid `any`. If you must use it for incremental migration, add a `TODO` with rationale.

## 6. Imports & Module Boundaries

- Prefer path aliases using Vite/TS config:
  - Use `"~"` for app-root imports (e.g. `"~/utils/tryCatch"`, `"~/services/api.server/apiClient"`).
  - Keep relative imports short within a feature folder when they do not cross major boundaries.
- Import ordering (follow existing style):
  - External packages (React, Remix, libraries).
  - Absolute app imports via `"~"`.
  - Relative imports (`"./"`, `"../"`).
- Naming:
  - Default exports for main components in a file (e.g. `TicketTable`, `RouteLayout`).
  - Named exports for helpers (`JsonResponse`, `ActionResponse`, `ForbiddenResponse`).

## 7. React / Remix Conventions

- Components:
  - Use function components with `export default function ComponentName() { ... }`.
  - Props types are usually a `type` alias named `ComponentNameProps`.
  - Keep JSX markup semantic and rely on Tailwind + DaisyUI utility classes for styling.
- Remix routes:
  - Route modules live under `client/app/routes` with nested folder naming mirroring the URL structure.
  - Use `RouteLayout` (`client/app/layouts/RouteLayout.tsx`) to wrap route content with consistent padding and background when appropriate.
  - Use Remix primitives (`Form`, `Link`, `Outlet`, `Meta`, `Links`, `Scripts`, `ScrollRestoration`) as in `client/app/root.tsx` and route files.
- Accessibility:
  - Maintain ARIA attributes and accessible labels when modifying buttons, forms, or interactive elements.
  - The `RouteChangeAnnouncement` helper in `root.tsx` exists to improve screen-reader behavior; do not remove it.

## 8. Styling, Tailwind, and UI

- Styling stack:
  - Tailwind CSS 4 + DaisyUI components are configured via `tailwind.config.js` (currently mostly empty).
  - Global styles live in `client/app/app.css` (imported in `root.tsx`).
- Class usage:
  - Compose utility classes directly in JSX via `className` (e.g. `"table table-zebra w-full"`, `"btn btn-xs btn-ghost"`).
  - Prefer existing DaisyUI semantics (`btn`, `badge-*`, `avatar`, etc.) for consistent look and feel.
- Layout:
  - Use flexbox and Tailwind layout utilities rather than custom inline styles.
  - Keep pages responsive; avoid hard-coded widths unless necessary.

## 9. Naming Conventions

- Files:
  - React components in `PascalCase` (e.g. `TicketTable.tsx`, `RouteLayout.tsx`).
  - Utility modules in `camelCase` or descriptive kebab where already used (e.g. `dateTime.ts`, `response.ts`).
- Types and interfaces:
  - `PascalCase` for exported types/interfaces (`ProjectResponse`, `ActionResponseResult`).
- Functions and variables:
  - `camelCase` (`getCompanyInfo`, `formatDateTimeShort`, `tryCatch`).
  - Avoid abbreviations unless conventional (`id`, `url`, `API`, `DTO`).
- Enums / string unions:
  - Prefer string literal unions or `enum` types defined in `types.ts` when status/priority values are reused.

## 10. Error Handling Patterns

- HTTP / API:
  - Use the base client in `client/app/services/api.server/baseClient.ts` for server interaction.
  - Use `tryCatch` (`client/app/utils/tryCatch.ts`) to convert `Promise` rejections into `{ data, error }` results when working at the API layer.
  - Map low-level failures to domain-specific errors like `AuthenticationError`, `TokenRefreshError`, and `ApiError` (`client/app/services/api.server/errors.ts`).
- Server responses in Remix:
  - Use `JsonResponse` and `ActionResponse` helpers from `client/app/utils/response.ts` to standardize JSON responses from loaders/actions.
  - Use `ForbiddenResponse` for 403 cases instead of hand-rolling `new Response()` where possible.
- Logging:
  - `BaseApiClient` includes `logResponse` that logs non-production responses; reuse that path instead of ad-hoc `console.log` for HTTP calls.

## 11. Data Fetching & API Service

- Prefer using the singleton API client exported from `client/app/services/api.server/apiClient.ts`.
- Keep all API endpoint URLs centralized in API service classes (`ApiService`, `AuthClient`, etc.).
- When adding a new API method:
  - Extend `ApiService` with a well-typed method that either returns `Response` or a typed JSON payload.
  - Use `requestJson<T>()` when parsing JSON; use `handleResponse()` when returning a raw `Response`.
  - Ensure authorization headers and base URL handling follow existing patterns.

## 12. Permissions, Roles, and Business Logic

- Business constants (roles, permissions, routes) live under `client/app/data/`.
- When extending roles or permissions, update the respective modules and keep their naming readable and explicit.
- Keep business rules (e.g. who can see or edit a ticket) near the route or service that uses them, not spread across components.

## 13. UI Components Example (TicketTable)

- `client/app/components/TicketTable.tsx` illustrates common UI patterns:
  - Props are defined as a `type` alias (`TicketTableProps`).
  - Data types (`BasicTicketInfo`) come from the API types module.
  - Helper functions (`getPriorityClass`, `getStatusClass`, `getPriorityDisplay`, `getStatusDisplay`, `getTypeDisplay`) are pure and file-local.
  - Use descriptive `className` strings composed from Tailwind + DaisyUI.
  - When adding new components, follow this pattern for structure and naming.

## 14. Cursor / Copilot Rules

- As of this writing there are **no** repo-specific rules files detected at:
  - `.cursor/rules/`
  - `.cursorrules`
  - `.github/copilot-instructions.md`
- If you introduce any of these, update this section to summarize the key expectations (tone, safety constraints, file ownership, etc.).

## 15. When Editing as an Agent

- Prefer minimal, surgical changes that respect surrounding style.
- Do not reformat entire files unless you also run `pnpm lint --fix` and ensure the project still builds.
- When adding new commands (scripts) to `client/package.json`, document them in this file under the appropriate section.
- If you introduce a new test setup, include example commands for:
  - Running the full test suite.
  - Running a single test or test file.

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
