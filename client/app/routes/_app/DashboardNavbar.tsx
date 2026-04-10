import { Form, Link, useFetcher, useMatches, type UIMatch } from "@remix-run/react";
import { useEffect, useRef, useState, type KeyboardEvent, type ReactNode } from "react";
import DropdownMenu from "~/components/DropdownMenu";
import type { CompanySearchResult } from "~/services/api.server/types";

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
  const searchFetcher = useFetcher<{ data: CompanySearchResult[]; error: string | null }>();
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
  const [searchQuery, setSearchQuery] = useState("");
  const [searchOpen, setSearchOpen] = useState(false);
  const searchRootRef = useRef<HTMLDivElement>(null);
  const searchFetcherRef = useRef(searchFetcher);
  const trimmedSearchQuery = searchQuery.trim();
  const searchResults = searchFetcher.data?.data ?? [];
  const shouldShowSearchDropdown = searchOpen && trimmedSearchQuery.length >= 2;
  const searchError = searchFetcher.data?.error;

  useEffect(() => {
    searchFetcherRef.current = searchFetcher;
  }, [searchFetcher]);

  useEffect(() => {
    if (trimmedSearchQuery.length < 2) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      searchFetcherRef.current.load(`/search?query=${encodeURIComponent(trimmedSearchQuery)}`);
    }, 200);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [trimmedSearchQuery]);

  useEffect(() => {
    function handlePointerDown(event: MouseEvent | TouchEvent) {
      if (!searchRootRef.current?.contains(event.target as Node)) {
        setSearchOpen(false);
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("touchstart", handlePointerDown);

    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("touchstart", handlePointerDown);
    };
  }, []);

  function getSearchResultHref(result: CompanySearchResult) {
    return result.type === "ticket" && result.projectId
      ? `/projects/${result.projectId}/tickets/${result.id}`
      : `/projects/${result.id}`;
  }

  function getSearchResultTypeLabel(result: CompanySearchResult) {
    return result.type === "ticket" ? "Ticket" : "Project";
  }

  function handleSearchFocus() {
    if (trimmedSearchQuery.length >= 2) {
      setSearchOpen(true);
    }
  }

  function handleSearchChange(value: string) {
    setSearchQuery(value);
    setSearchOpen(value.trim().length >= 2);
  }

  function handleSearchKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Escape") {
      setSearchOpen(false);
      event.currentTarget.blur();
    }
  }

  function handleSearchResultClick() {
    setSearchQuery("");
    setSearchOpen(false);
  }

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
            <div className="relative w-full max-w-md" ref={searchRootRef}>
              <div className="group relative w-full min-w-0 rounded-lg bg-[var(--app-surface-container-lowest)] shadow-[var(--app-search-shadow)] transition-[background-color,box-shadow] duration-200 focus-within:shadow-[var(--app-search-focus-shadow)]">
                <span className="material-symbols-outlined pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-sm text-[color:var(--app-outline)]">
                  search
                </span>
                <input
                  aria-label="Search tickets and projects"
                  aria-controls="dashboard-navbar-search-results"
                  className={`${monoClass} h-9 w-full rounded-lg border-none bg-transparent pl-10 pr-10 text-xs tracking-wide text-[var(--app-on-surface)] outline-none placeholder:text-[color:var(--app-outline)]`}
                  onChange={(event) => handleSearchChange(event.target.value)}
                  onFocus={handleSearchFocus}
                  onKeyDown={handleSearchKeyDown}
                  placeholder="Search tickets, projects..."
                  type="text"
                  value={searchQuery}
                />
                {searchFetcher.state !== "idle" ? (
                  <span className="absolute right-3 top-1/2 h-2 w-2 -translate-y-1/2 rounded-full bg-[var(--app-primary)] animate-pulse" />
                ) : null}
              </div>

              {shouldShowSearchDropdown ? (
                <div
                  className="absolute left-0 right-0 top-[calc(100%+0.5rem)] z-30 overflow-hidden rounded-2xl bg-[var(--app-surface-container-menu)] outline outline-1 outline-[var(--app-outline-variant-strong)] shadow-[var(--app-menu-shadow)]"
                  id="dashboard-navbar-search-results"
                  role="listbox"
                >
                  {searchError ? (
                    <div className="px-4 py-3 text-sm text-[var(--app-error)]">{searchError}</div>
                  ) : null}

                  {!searchError && searchFetcher.state !== "idle" && searchResults.length === 0 ? (
                    <div className="px-4 py-3 text-sm text-[var(--app-on-surface-variant)]">Searching...</div>
                  ) : null}

                  {!searchError && searchFetcher.state === "idle" && searchResults.length === 0 ? (
                    <div className="px-4 py-3 text-sm text-[var(--app-on-surface-variant)]">No tickets or projects found.</div>
                  ) : null}

                  {searchResults.length > 0 ? (
                    <div className="divide-y divide-[color:var(--app-outline-variant)]/10">
                      {searchResults.map((result) => (
                        <Link
                          className="flex items-start justify-between gap-3 px-4 py-3 transition-colors hover:bg-[var(--app-surface-container-high)]"
                          key={`${result.type}-${result.id}`}
                          onClick={handleSearchResultClick}
                          to={getSearchResultHref(result)}
                        >
                          <div className="min-w-0">
                            <div className="flex items-center gap-2 text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">
                              <span className={monoClass}>{getSearchResultTypeLabel(result)}</span>
                              {result.displayId ? <span className={monoClass}>{result.displayId}</span> : null}
                            </div>
                            <div className="mt-1 truncate text-sm font-medium text-[var(--app-on-surface)]">{result.name}</div>
                          </div>
                          <span className="material-symbols-outlined mt-0.5 text-base text-[var(--app-outline)]">arrow_outward</span>
                        </Link>
                      ))}
                    </div>
                  ) : null}
                </div>
              ) : null}
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

          <DropdownMenu
            menuClassName="w-56"
            triggerAriaLabel="Open account menu"
            triggerClassName="inline-flex h-9 w-9 items-center justify-center rounded-lg transition-colors hover:bg-[var(--app-surface-container-high)]"
            trigger={
              <span className="inline-flex h-8 w-8 items-center justify-center overflow-hidden rounded-full border border-[var(--app-avatar-border)] transition-transform hover:-translate-y-0.5">
                {avatarUrl ? (
                  <img alt="Developer avatar" className="h-full w-full object-cover" src={avatarUrl} />
                ) : (
                  <span className={`${monoClass} text-[11px] font-medium text-[var(--app-on-surface)]`}>ZA</span>
                )}
              </span>
            }
          >
            {({ close }) => (
              <>
                <button
                  className={accountMenuItemClass}
                  onClick={close}
                  type="button"
                >
                  <span className="material-symbols-outlined text-lg">person</span>
                  <span>Profile</span>
                </button>

                <button
                  className={accountMenuItemClass}
                  onClick={close}
                  type="button"
                >
                  <span className="material-symbols-outlined text-lg">settings</span>
                  <span>Settings</span>
                </button>

                <Form method="post" onSubmit={close}>
                  <button
                    className={accountMenuItemClass}
                    formAction="/logout"
                    type="submit"
                  >
                    <span className="material-symbols-outlined text-lg">logout</span>
                    <span>Logout</span>
                  </button>
                </Form>
              </>
            )}
          </DropdownMenu>
        </div>
      </div>
    </header>
  );
}
