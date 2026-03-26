import { Form, useMatches, type UIMatch } from "@remix-run/react";
import type { ReactNode } from "react";

type DashboardNavbarProps = {
  avatarUrl: string;
  onMenuToggle?: () => void;
};

type BreadcrumbMatch = UIMatch<unknown, {
  breadcrumb?: (match: UIMatch) => ReactNode;
  breadcrumbLabel?: string | ((match: UIMatch) => string);
}>;

const monoClass = "[font-family:'JetBrains_Mono',monospace]";

const utilityButtonClass =
  "inline-flex h-9 w-9 cursor-pointer items-center justify-center rounded-lg text-[color:var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-surface-container-high)] hover:text-[var(--app-on-surface)]";

const accountMenuItemClass =
  "flex w-full cursor-pointer items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm text-[color:var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]";

function normalizeBreadcrumbLabel(label: string | null) {
  return label?.trim().toLowerCase() ?? null;
}

function getBreadcrumbLabel(match: BreadcrumbMatch) {
  const breadcrumbLabel = match.handle?.breadcrumbLabel;

  if (typeof breadcrumbLabel === "function") {
    return breadcrumbLabel(match);
  }

  return breadcrumbLabel ?? match.pathname;
}

export default function DashboardNavbar({ avatarUrl, onMenuToggle }: DashboardNavbarProps) {
  const matches = useMatches() as BreadcrumbMatch[];
  const breadcrumbMatches = matches
    .filter((match) => match.handle && match.handle.breadcrumb)
    .filter((match, index, allMatches) => {
      if (index === 0) {
        return true;
      }

      return (
        normalizeBreadcrumbLabel(getBreadcrumbLabel(match)) !==
        normalizeBreadcrumbLabel(getBreadcrumbLabel(allMatches[index - 1]))
      );
    });

  return (
    <header
      className="sticky top-0 z-40 w-full backdrop-blur-[24px]"
      style={{ backgroundColor: "var(--app-surface-container-overlay)", boxShadow: "var(--app-topbar-shadow)" }}
    >
      <div className="flex min-h-14 items-start justify-between gap-4 px-4 py-2.5 sm:px-6 lg:px-8">
        <div className="flex min-w-0 flex-1 items-start gap-4 lg:gap-6">
          <button
            aria-label="Open menu"
            className={`${utilityButtonClass} mt-0.5 lg:hidden`}
            onClick={onMenuToggle}
            type="button"
          >
            <span className="material-symbols-outlined text-xl">menu</span>
          </button>

          <div className="min-w-0 flex-1 space-y-2">
            <div className="group relative w-full min-w-0 max-w-md rounded-lg bg-[var(--app-surface-container-lowest)] shadow-[var(--app-search-shadow)] transition-[background-color,box-shadow] duration-200 focus-within:shadow-[var(--app-search-focus-shadow)]">
              <span className="material-symbols-outlined pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-sm text-[color:var(--app-outline)]">
                search
              </span>
              <input
                aria-label="Search tickets and projects"
                className={`${monoClass} h-9 w-full rounded-lg border-none bg-transparent pl-10 pr-4 text-xs tracking-wide text-[var(--app-on-surface)] outline-none placeholder:text-[color:var(--app-outline)]`}
                placeholder="Search tickets, projects..."
                type="text"
              />
            </div>

            <nav aria-label="Breadcrumb" className="hidden min-w-0 items-center gap-2 text-xs lg:flex">
              {breadcrumbMatches.map((match, index) => (
                <div className="flex min-w-0 mt-2 items-center gap-2" key={`${match.id}-${getBreadcrumbLabel(match)}`}>
                  {index > 0 ? (
                    <span className={`${monoClass} text-[10px] text-[color:var(--app-outline)]`}>/</span>
                  ) : null}
                  <span className="min-w-0 truncate text-[color:var(--app-on-surface-variant)] [&_a]:truncate [&_a]:text-inherit [&_a]:transition-colors [&_a]:hover:text-[var(--app-on-surface)]">
                    {match.handle?.breadcrumb?.(match)}
                  </span>
                </div>
              ))}
            </nav>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <button
            aria-label="notifications"
            className={utilityButtonClass}
            type="button"
          >
            <span className="material-symbols-outlined text-xl">notifications</span>
          </button>

          <div className="hidden h-6 w-px bg-[color:var(--app-outline-variant)]/20 sm:block" />

          <details className="relative">
            <summary className="flex list-none">
              <span className="inline-flex h-9 w-9 cursor-pointer items-center justify-center rounded-lg transition-colors hover:bg-[var(--app-surface-container-high)]">
                <span className="inline-flex h-8 w-8 items-center justify-center overflow-hidden rounded-full border border-[var(--app-avatar-border)] transition-transform hover:-translate-y-0.5">
                  {avatarUrl ? (
                    <img alt="Developer avatar" className="h-full w-full object-cover" src={avatarUrl} />
                  ) : (
                    <span className={`${monoClass} text-[11px] font-medium text-[var(--app-on-surface)]`}>ZA</span>
                  )}
                </span>
              </span>
            </summary>

            <div
              className="absolute right-0 mt-3 w-56 rounded-2xl bg-[var(--app-surface-container-menu)] p-2.5 outline outline-1 outline-[var(--app-outline-variant-strong)] shadow-[var(--app-menu-shadow)]"
            >
              <button
                className={accountMenuItemClass}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">person</span>
                <span>Profile</span>
              </button>

              <button
                className={accountMenuItemClass}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">settings</span>
                <span>Settings</span>
              </button>

              <Form method="post">
                <button
                  className={accountMenuItemClass}
                  formAction="/logout"
                  type="submit"
                >
                  <span className="material-symbols-outlined text-lg">logout</span>
                  <span>Logout</span>
                </button>
              </Form>
            </div>
          </details>
        </div>
      </div>
    </header>
  );
}
