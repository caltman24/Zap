import { Outlet, useLoaderData, useLocation, useNavigation } from "@remix-run/react";
import { useEffect, useState } from "react";
import { redirect, type LoaderFunctionArgs } from "@remix-run/node";
import DashboardNavbar from "./DashboardNavbar";
import SideMenu from "./SideMenu";
import { filterMenuRoutesByPermissions, menuRoutes } from "~/data/routes";
import apiClient from "~/services/api.server/apiClient";
import type { CompanyInfoResponse, UserInfoResponse } from "~/services/api.server/types";
import { commitSession, destroySession, getSession } from "~/services/sessions.server";
import { JsonResponse, type JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

const mobileMenuTransitionMs = 220;
const monoClass = "[font-family:'JetBrains_Mono',monospace]";

type AppShellData = {
  user: UserInfoResponse;
  company: Pick<CompanyInfoResponse, "name" | "logoUrl"> | null;
};

export async function loader({ request }: LoaderFunctionArgs) {
  const session = await getSession(request);
  const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  const accessToken = tokenResponse.token;

  const newUserInfo = await apiClient.getUserInfo(accessToken);

  if (!newUserInfo) {
    return redirect("/login", {
      headers: {
        "Set-Cookie": await destroySession(session),
      },
    });
  }

  if (!newUserInfo.companyId) {
    return redirect("/setup", {
      headers: {
        "Set-Cookie": await commitSession(session),
      },
    });
  }

  session.set("user", newUserInfo);

  const { data: companyInfo } = await tryCatch(apiClient.getCompanyInfo(accessToken));

  return JsonResponse({
    data: {
      user: newUserInfo,
      company: companyInfo
        ? {
            name: companyInfo.name,
            logoUrl: companyInfo.logoUrl,
          }
        : null,
    },
    error: null,
    headers: {
      "Set-Cookie": await commitSession(session),
    },
  });
}

export default function AppRoute() {
  const { data: appShellData } = useLoaderData<typeof loader>() as JsonResponseResult<AppShellData>;
  const userData = appShellData?.user;
  const navigation = useNavigation();
  const location = useLocation();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [mobileMenuMounted, setMobileMenuMounted] = useState(false);
  const filteredRoutes = filterMenuRoutesByPermissions(menuRoutes, userData?.permissions ?? [], userData?.role);

  function openMobileMenu() {
    setMobileMenuMounted(true);
    window.requestAnimationFrame(() => setMobileMenuOpen(true));
  }

  function closeMobileMenu() {
    setMobileMenuOpen(false);
  }

  useEffect(() => {
    closeMobileMenu();
  }, [location.pathname]);

  useEffect(() => {
    if (!mobileMenuMounted) {
      return undefined;
    }

    const { overflow } = document.body.style;
    document.body.style.overflow = "hidden";

    return () => {
      document.body.style.overflow = overflow;
    };
  }, [mobileMenuMounted]);

  useEffect(() => {
    if (!mobileMenuMounted) {
      return undefined;
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        closeMobileMenu();
      }
    }

    window.addEventListener("keydown", handleKeyDown);

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [mobileMenuMounted]);

  useEffect(() => {
    if (!mobileMenuMounted || mobileMenuOpen) {
      return undefined;
    }

    const timeoutId = window.setTimeout(() => {
      setMobileMenuMounted(false);
    }, mobileMenuTransitionMs);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [mobileMenuMounted, mobileMenuOpen]);

  return (
    <div className="app-shell">
      <div className="min-h-screen lg:pl-[15.5rem]">
        <div className="hidden outline outline-1 outline-[var(--app-outline-variant-soft)] lg:fixed lg:inset-y-0 lg:left-0 lg:z-50 lg:flex lg:w-[15.5rem]">
          <SideMenu company={appShellData?.company ?? null} menuRoutes={filteredRoutes} />
        </div>

        {mobileMenuMounted ? (
          <div className="fixed inset-0 z-50 lg:hidden" role="dialog" aria-modal="true">
            <button
              aria-label="Close menu"
              className={`absolute inset-0 bg-[var(--app-overlay-backdrop)] backdrop-blur-sm transition-opacity duration-200 ${mobileMenuOpen ? "opacity-100" : "opacity-0"}`}
              onClick={closeMobileMenu}
              type="button"
            />

            <div
              className={`absolute inset-y-0 left-0 flex w-[15.5rem] max-w-[85vw] transition-transform duration-[220ms] ease-out will-change-transform ${mobileMenuOpen ? "translate-x-0" : "-translate-x-full"}`}
            >
              <SideMenu
                company={appShellData?.company ?? null}
                menuRoutes={filteredRoutes}
                onClose={closeMobileMenu}
                onNavigate={closeMobileMenu}
              />
            </div>
          </div>
        ) : null}

        <div className="flex min-h-screen min-w-0 flex-col">
          <DashboardNavbar avatarUrl={userData?.avatarUrl ?? ""} onMenuToggle={openMobileMenu} />

          <div className="relative min-h-0 flex-1 overflow-y-auto">
            <div className="app-shell-scroll flex min-h-full flex-col">
              <div className="flex-1">
                <Outlet context={userData!} />
              </div>
            </div>

            {navigation.state === "loading" ? (
              <div className="pointer-events-none absolute inset-0 grid place-items-center bg-[var(--app-loading-overlay)] backdrop-blur-[2px]">
                <div className="flex items-center gap-3 rounded-full bg-[var(--app-surface-container-panel)] px-4 py-3 text-sm text-[var(--app-on-surface)] outline outline-1 outline-[var(--app-outline-variant-faint)] shadow-[var(--app-panel-shadow)]">
                  <span className="h-2.5 w-2.5 animate-pulse rounded-full bg-[var(--app-primary)]" />
                  <span className={`${monoClass} text-[11px] uppercase tracking-[0.22em] text-[color:var(--app-on-surface-variant)]`}>
                    Loading
                  </span>
                </div>
              </div>
            ) : null}
          </div>
        </div>
      </div>
    </div>
  );
}
