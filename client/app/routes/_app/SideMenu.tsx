import { Form, Link, NavLink, useLocation, useMatches } from "@remix-run/react";
import { MenuLink, MenuRoutes } from "~/data/routes";

type SideMenuProps = {
  menuRoutes: MenuRoutes;
  onNavigate?: () => void;
  onClose?: () => void;
};

const railItemBaseClass =
  "group relative flex min-h-12 w-full cursor-pointer items-center gap-3 rounded-2xl px-4 py-3 text-left text-sm text-[color:var(--app-on-surface-variant)] transition-all duration-200 hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)] active:scale-95";

const railItemActiveClass =
  "text-[var(--app-primary-fixed-strong)] before:absolute before:left-0 before:top-1/2 before:h-6 before:w-0.5 before:-translate-y-1/2 before:bg-[var(--app-primary-fixed-strong)] before:content-[''] before:shadow-[var(--app-active-rail-shadow)]";

const railIconBaseClass = "material-symbols-outlined shrink-0 text-[22px] text-[var(--app-rail-icon)]";

const railLabelClass = "flex-1 overflow-hidden text-ellipsis whitespace-nowrap text-[0.84rem] leading-[1.1rem]";

const utilityButtonClass =
  "inline-flex h-9 w-9 cursor-pointer items-center justify-center rounded-lg text-[color:var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-surface-container-high)] hover:text-[var(--app-on-surface)]";

function isPrimaryCollectionLink(link: MenuLink) {
  return link.to === "/projects" || link.to === "/tickets";
}

export default function SideMenu({ menuRoutes, onNavigate, onClose }: SideMenuProps) {
  const matches = useMatches();
  const location = useLocation();

  function isLinkForcedInactive(link: MenuLink) {
    return matches.some(
      (match) =>
        (match.id.includes("archived") ||
          match.id.includes("mytickets") ||
          match.id.includes("myprojects")) &&
        isPrimaryCollectionLink(link),
    );
  }

  function isLinkCurrentlyActive(link: MenuLink, isActive: boolean) {
    if (isLinkForcedInactive(link)) {
      return false;
    }

    return isActive || matches.some((match) => match.id === link.matchId);
  }

  return (
    <aside className="flex h-full w-full flex-col bg-[var(--app-surface)] py-5 text-[var(--app-on-surface)] lg:w-[15.5rem] lg:py-6">
      <div className="mb-7 px-5 lg:px-7">
        <div className="flex items-center justify-between gap-3">
          <Link
            className="app-shell-headline text-[2.15rem] font-black tracking-[-0.08em] text-[var(--app-primary-fixed-strong)] transition-colors hover:text-[var(--app-primary-fixed)]"
            onClick={onNavigate}
            to="/"
          >
            Zap
          </Link>

          {onClose ? (
            <button
              aria-label="Close menu"
              className={`${utilityButtonClass} lg:hidden`}
              onClick={onClose}
              type="button"
            >
              <span className="material-symbols-outlined text-xl">close</span>
            </button>
          ) : null}
        </div>
      </div>

      <div className="app-shell-scroll flex-1 overflow-y-auto px-3 lg:px-4">
        <nav aria-label="Primary" className="flex flex-col gap-6">
          {menuRoutes.map((group, index) => {
            if (group.length === 0) {
              return null;
            }

            return (
              <div className="flex flex-col gap-2.5" key={group[0]?.to ?? index}>
                {group.map((link) => {
                  const preserveTicketSearch =
                    location.pathname.startsWith("/tickets") &&
                    link.to.startsWith("/tickets") &&
                    link.to !== "/tickets/new";

                  return (
                    <NavLink
                      key={link.to}
                      className={({ isActive }) => {
                        const active = isLinkCurrentlyActive(link, isActive);

                        return [railItemBaseClass, active ? railItemActiveClass : ""].join(" ");
                      }}
                      end={!matches.some((match) => match.id.endsWith("Id"))}
                      onClick={onNavigate}
                      to={{
                        pathname: link.to,
                        search: preserveTicketSearch ? location.search : "",
                      }}
                    >
                      {({ isActive }) => {
                        const active = isLinkCurrentlyActive(link, isActive);

                        return (
                          <>
                            {link.materialIcon ? (
                              <span
                                className={`${railIconBaseClass} ${active ? "text-[var(--app-primary-fixed-strong)]" : "group-hover:text-[var(--app-on-surface)]"}`}
                              >
                                {link.materialIcon}
                              </span>
                            ) : null}
                            <span className={`${railLabelClass} ${active ? "text-[var(--app-primary-fixed-strong)]" : ""}`}>{link.name}</span>
                          </>
                        );
                      }}
                    </NavLink>
                  );
                })}
              </div>
            );
          })}
        </nav>
      </div>

      <div className="mt-auto flex flex-col gap-3 px-3 pb-1 lg:px-4">
        <div className="h-px w-full bg-[var(--app-outline-variant)]/20" />

        <div className="flex flex-col gap-2.5">
          <Form method="post">
            <button
              className={railItemBaseClass}
              formAction="/logout"
              type="submit"
            >
              <span className={`${railIconBaseClass} group-hover:text-[var(--app-on-surface)]`}>logout</span>
              <span className={railLabelClass}>Logout</span>
            </button>
          </Form>
        </div>
      </div>
    </aside>
  );
}
