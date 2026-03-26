import { redirect, type LoaderFunctionArgs } from "@remix-run/node";
import { Link, useLoaderData, useNavigate, useOutletContext } from "@remix-run/react";
import type { KeyboardEvent } from "react";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import type { BasicTicketInfo, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { requestJson } from "~/utils/api";
import { JsonResponse, type JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import roleNames from "~/data/roles";
import { hasPermission } from "~/utils/permissions";
import getMyProjects from "../_app.projects.myprojects._index/server.get-myprojects";
import { getMyTickets } from "../_app.tickets.mytickets._index/server.get-mytickets";
import {
  type DashboardData,
  formatDashboardDateParts,
  formatDeadlineDays,
  formatRelativeTime,
  getDashboardDeadlineLabel,
  getDashboardInitials,
  getDashboardProjectLabel,
  getDashboardSummaryTickets,
  getDashboardTicketLabels,
  getDeadlineTone,
  getPriorityDotClass,
  getStatusChipClass,
  getTicketUpdatedAt,
  toDashboardActivityItems,
  toUpcomingDeadlines,
  truncateDashboardText,
} from "./dashboardUtils";

const dashboardFilters = [
  {
    icon: "filter_list",
    label: "Status: All",
    options: ["Status: All", "New", "In Development", "Testing", "Resolved"],
  },
  {
    icon: "bolt",
    label: "Priority: All",
    options: ["Priority: All", "High", "Medium", "Low"],
  },
  {
    icon: "person",
    label: "Assignee: Me",
    options: ["Assignee: Me", "Team Alpha", "Unassigned"],
  },
] as const;

export async function loader({ request }: LoaderFunctionArgs) {
  const session = await getSession(request);
  const userInfo = session.get("user") as UserInfoResponse;
  const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  try {
    const useOwnTicketSummary = [roleNames.developer, roleNames.submitter].includes(userInfo.role);
    const projectsRequest = hasPermission(userInfo.permissions, "project.viewAll")
      ? apiClient.getCompanyProjects(tokenResponse.token)
      : getMyProjects(userInfo.memberId!, tokenResponse.token);
    const ownTicketsRequest = useOwnTicketSummary
      ? getMyTickets(tokenResponse.token)
      : Promise.resolve<BasicTicketInfo[] | null>(null);

    const [projects, openTickets, resolvedTickets, ownTickets] = await Promise.all([
      projectsRequest,
      requestJson<BasicTicketInfo[]>("/tickets/open", { method: "GET" }, tokenResponse.token),
      requestJson<BasicTicketInfo[]>("/tickets/resolved", { method: "GET" }, tokenResponse.token),
      ownTicketsRequest,
    ]);

    const summaryTickets = getDashboardSummaryTickets(userInfo.role, userInfo.memberId, ownTickets);

    const ownOpenTickets = summaryTickets?.filter((ticket) => ticket.status.toLowerCase() !== "resolved") ?? [];
    const ownResolvedTickets = summaryTickets?.filter((ticket) => ticket.status.toLowerCase() === "resolved") ?? [];
    const upcomingDeadlines = toUpcomingDeadlines(projects);

    const recentActivity = [...openTickets, ...resolvedTickets]
      .sort((left, right) => getTicketUpdatedAt(right) - getTicketUpdatedAt(left))
      .slice(0, 5);

    const dashboardData: DashboardData = {
      totalProjects: projects.length,
      totalTickets: useOwnTicketSummary ? summaryTickets?.length ?? 0 : openTickets.length + resolvedTickets.length,
      openTickets: useOwnTicketSummary ? ownOpenTickets.length : openTickets.length,
      closedTickets: useOwnTicketSummary ? ownResolvedTickets.length : resolvedTickets.length,
      recentActivity,
      upcomingDeadlines,
    };

    return JsonResponse({
      data: dashboardData,
      error: null,
      headers: tokenResponse.headers,
    });
  } catch (error: unknown) {
    if (error instanceof AuthenticationError) {
      return redirect("/logout");
    }

    const errorMessage = error instanceof Error ? error.message : "Failed to load dashboard data.";

    return JsonResponse({
      data: null,
      error: errorMessage,
      headers: tokenResponse.headers,
    });
  }
}

export const handle = {
  breadcrumb: () => <Link to="/dashboard">Dashboard</Link>,
  breadcrumbLabel: "Dashboard",
};

export default function DashboardRoute() {
  const { data, error } = useLoaderData<JsonResponseResult<DashboardData>>();
  const userInfo = useOutletContext<UserInfoResponse>();
  const userRole = userInfo.role.toLowerCase() as UserInfoResponse["role"];
  const navigate = useNavigate();
  const projectLabel = getDashboardProjectLabel(userRole);
  const { ticketLabel, openTicketLabel, closedTicketLabel } = getDashboardTicketLabels(userInfo.role);
  const deadlineLabel = getDashboardDeadlineLabel(userRole);
  const activityItems = data ? toDashboardActivityItems(data.recentActivity, userInfo) : [];

  const statCards = data
    ? [
        { label: projectLabel, value: data.totalProjects, mono: false, accentClass: "border-l-[var(--app-primary-fixed-strong)]" },
        { label: ticketLabel, value: data.totalTickets, mono: false, accentClass: "border-l-[var(--app-secondary)]" },
        { label: openTicketLabel, value: data.openTickets, mono: false, accentClass: "border-l-[var(--app-tertiary)]" },
        { label: closedTicketLabel, value: data.closedTickets, mono: false, accentClass: "border-l-[var(--app-success)]" },
      ]
    : [];

  function getTicketRoute(ticket: BasicTicketInfo): string {
    return `/projects/${ticket.projectId}/tickets/${ticket.id}`;
  }

  function handleTicketRowKeyDown(event: KeyboardEvent<HTMLTableRowElement>, route: string): void {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      navigate(route);
    }
  }

  if (error || !data) {
    return (
      <RouteLayout>
        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
          {error ?? "Unable to load dashboard data."}
        </div>
      </RouteLayout>
    );
  }

  return (
    <RouteLayout className="space-y-8 pb-8">
      <h1 className="sr-only">Dashboard</h1>

      <section className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {statCards.map((card) => {
          return (
            <article
              className={`rounded-2xl border-l-2 bg-[var(--app-surface-container-low)] p-5 outline outline-1 outline-[var(--app-outline-variant-soft)] ${card.accentClass}`}
              key={card.label}
            >
              <div className="mb-6 flex items-center justify-between gap-3">
                <span className="app-shell-mono text-xs text-[var(--app-outline)]">{card.label}</span>
              </div>
              <div className={`text-[2.1rem] font-bold tracking-[-0.04em] text-[var(--app-on-surface)] ${card.mono ? "app-shell-mono" : ""}`}>
                {card.value}
              </div>
            </article>
          );
        })}
      </section>

      <section className="overflow-hidden rounded-[1.75rem] bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
        <div className="flex flex-col gap-4 bg-[color:var(--app-surface-container-high)]/50 p-4 md:flex-row md:items-center md:justify-between">
          <div className="flex flex-wrap items-center gap-3">
            {dashboardFilters.map((filter) => (
              <label
                className="flex items-center gap-2 rounded-xl bg-[var(--app-surface-container-lowest)] px-3 py-2 text-xs text-[var(--app-on-surface)] outline outline-1 outline-[var(--app-outline-variant-soft)]"
                key={filter.label}
              >
                <span className="material-symbols-outlined text-sm text-[var(--app-outline)]">{filter.icon}</span>
                <select
                  aria-label={filter.label}
                  className="cursor-pointer border-none bg-transparent pr-6 text-xs text-[var(--app-on-surface)] outline-none focus:ring-0"
                  defaultValue={filter.label}
                >
                  {filter.options.map((option) => (
                    <option key={option}>{option}</option>
                  ))}
                </select>
              </label>
            ))}
          </div>

          <Link
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2 text-xs font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95"
            to="/tickets/new"
          >
            <span className="material-symbols-outlined text-sm">add</span>
            New Issue
          </Link>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[880px] border-collapse text-left">
            <thead className="border-b border-[color:var(--app-outline-variant)]/10 bg-[var(--app-surface-container-lowest)]">
              <tr>
                <th className="px-6 py-4 text-[10px] font-medium uppercase tracking-[0.28em] text-[var(--app-outline)]">Title &amp; Preview</th>
                <th className="px-6 py-4 text-[10px] font-medium uppercase tracking-[0.28em] text-[var(--app-outline)]">Status</th>
                <th className="px-6 py-4 text-[10px] font-medium uppercase tracking-[0.28em] text-[var(--app-outline)]">Priority</th>
                <th className="px-6 py-4 text-[10px] font-medium uppercase tracking-[0.28em] text-[var(--app-outline)]">Assignee</th>
                <th className="px-6 py-4 text-[10px] font-medium uppercase tracking-[0.28em] text-[var(--app-outline)]">Updated</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[color:var(--app-outline-variant)]/5">
              {data.recentActivity.length > 0 ? (
                data.recentActivity.map((ticket) => {
                  const route = getTicketRoute(ticket);
                  const assigneeName = ticket.assignee?.name ?? "Unassigned";

                  return (
                    <tr
                      className="group cursor-pointer transition-colors duration-150 hover:bg-[color:var(--app-surface-container-highest)]/40 focus-within:bg-[color:var(--app-surface-container-highest)]/40"
                      key={ticket.id}
                      onClick={() => navigate(route)}
                      onKeyDown={(event) => handleTicketRowKeyDown(event, route)}
                      tabIndex={0}
                    >
                      <td className="px-6 py-5 align-top">
                        <div className="flex flex-col gap-1">
                          <span className="text-[1.05rem] font-semibold text-[var(--app-on-surface)] transition-colors group-hover:text-[var(--app-primary)]">
                            {ticket.name}
                          </span>
                          <span className="text-xs text-[var(--app-on-surface-variant)]">
                            {truncateDashboardText(ticket.description, 78)}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-5 align-top">
                        <span className={`app-shell-mono inline-flex rounded-md px-2.5 py-1 text-[10px] uppercase tracking-[0.2em] ${getStatusChipClass(ticket.status)}`}>
                          {ticket.status}
                        </span>
                      </td>
                      <td className="px-6 py-5 align-top">
                        <div className="flex items-center gap-2 text-sm text-[var(--app-on-surface)]">
                          <span className={`h-2.5 w-2.5 rounded-full ${getPriorityDotClass(ticket.priority)}`} />
                          <span>{ticket.priority}</span>
                        </div>
                      </td>
                      <td className="px-6 py-5 align-top">
                        <div className="flex items-center gap-3">
                          {ticket.assignee?.avatarUrl ? (
                            <img
                              alt={assigneeName}
                              className="h-6 w-6 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                              src={ticket.assignee.avatarUrl}
                            />
                          ) : (
                            <span className="inline-flex h-6 w-6 items-center justify-center rounded-full bg-[var(--app-surface-container-high)] text-[10px] font-bold text-[var(--app-outline)]">
                              {getDashboardInitials(assigneeName)}
                            </span>
                          )}
                          <span className="text-sm text-[var(--app-on-surface)]">{assigneeName}</span>
                        </div>
                      </td>
                      <td className="px-6 py-5 align-top">
                        <span className="app-shell-mono text-xs text-[var(--app-outline)]">
                          {formatRelativeTime(ticket.updatedAt ?? ticket.createdAt)}
                        </span>
                      </td>
                    </tr>
                  );
                })
              ) : (
                <tr>
                  <td className="px-6 py-12 text-center text-sm text-[var(--app-on-surface-variant)]" colSpan={5}>
                    No recent ticket activity found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between bg-[var(--app-surface-container-lowest)] px-4 py-4">
          <span className="app-shell-mono text-xs text-[var(--app-outline)]">
            Showing 1-{data.recentActivity.length} of {data.totalTickets} tickets
          </span>
          <div className="flex items-center gap-2">
            <button
              aria-label="Previous page"
              className="inline-flex h-8 w-8 items-center justify-center rounded-md text-[var(--app-outline)] transition-colors hover:bg-[var(--app-surface-container-high)]"
              type="button"
            >
              <span className="material-symbols-outlined text-sm">chevron_left</span>
            </button>
            <button
              aria-label="Next page"
              className="inline-flex h-8 w-8 items-center justify-center rounded-md text-[var(--app-outline)] transition-colors hover:bg-[var(--app-surface-container-high)]"
              type="button"
            >
              <span className="material-symbols-outlined text-sm">chevron_right</span>
            </button>
          </div>
        </div>
      </section>

      <section className="grid grid-cols-1 gap-8 lg:grid-cols-12">
        <div className="space-y-4 lg:col-span-5">
          <h2 className="flex items-center gap-2 text-xl font-bold tracking-tight text-[var(--app-on-surface)]">
            <span className="material-symbols-outlined text-[var(--app-secondary)]">event</span>
            {deadlineLabel}
          </h2>

          <div className="overflow-hidden rounded-2xl bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
            <div className="divide-y divide-[color:var(--app-outline-variant)]/5">
              {data.upcomingDeadlines.length > 0 ? (
                data.upcomingDeadlines.map((deadline) => {
                  const tone = getDeadlineTone(deadline);
                  const { month, day } = formatDashboardDateParts(deadline.dueDate);

                  return (
                    <button
                      className="group flex w-full items-center justify-between gap-4 p-4 text-left transition-colors hover:bg-[var(--app-surface-container-high)]/30"
                      key={deadline.id}
                      type="button"
                    >
                      <div className="flex min-w-0 items-center gap-4">
                        <div className={`flex h-10 w-10 shrink-0 flex-col items-center justify-center rounded-lg border ${tone.containerClass}`}>
                          <span className={`app-shell-mono text-[10px] uppercase ${tone.monthClass}`}>{month}</span>
                          <span className={`text-sm font-bold ${tone.dayClass}`}>{day}</span>
                        </div>
                        <div className="min-w-0">
                          <h3 className="truncate text-sm font-medium text-[var(--app-on-surface)] transition-colors group-hover:text-[var(--app-primary)]">
                            {deadline.name}
                          </h3>
                          <span className={`app-shell-mono text-xs ${tone.labelClass}`}>
                            {formatDeadlineDays(deadline.daysRemaining)}
                          </span>
                        </div>
                      </div>
                      <span className="material-symbols-outlined text-lg text-[var(--app-outline)] transition-transform group-hover:translate-x-1">
                        chevron_right
                      </span>
                    </button>
                  );
                })
              ) : (
                <div className="p-6 text-sm text-[var(--app-on-surface-variant)]">No project deadlines found.</div>
              )}
            </div>
          </div>
        </div>

        <div className="space-y-4 lg:col-span-7">
          <h2 className="flex items-center gap-2 text-xl font-bold tracking-tight text-[var(--app-on-surface)]">
            <span className="material-symbols-outlined text-[var(--app-primary)]">history_edu</span>
            Recent Activity
          </h2>

          <div className="rounded-2xl bg-[var(--app-surface-container-low)] p-6 outline outline-1 outline-[var(--app-outline-variant-soft)]">
            <div className="space-y-6">
              {activityItems.map((item, index) => {
                const showLine = index < activityItems.length - 1;

                if (item.variant === "commit") {
                  return (
                    <div className="relative flex gap-4" key={item.id}>
                      {showLine ? (
                        <div className="absolute bottom-0 left-4 top-10 w-px bg-[color:var(--app-outline-variant)]/10" />
                      ) : null}
                      <div className="z-10 flex h-8 w-8 items-center justify-center rounded-full border border-[var(--app-secondary)]/10 bg-[var(--app-secondary-container)]/30">
                        <span className="material-symbols-outlined text-sm text-[var(--app-secondary)]">{item.icon}</span>
                      </div>
                      <div className="min-w-0 flex-1 space-y-1">
                        <p className="text-sm text-[var(--app-on-surface)]">
                          New commit on <span className="app-shell-mono text-xs">{item.branchName}</span> by <span className="font-bold">{item.actorName}</span>
                        </p>
                        <p className="line-clamp-1 text-xs text-[var(--app-on-surface-variant)]">{item.detail}</p>
                        <span className="app-shell-mono text-[10px] text-[var(--app-outline)]">{item.timestamp}</span>
                      </div>
                    </div>
                  );
                }

                return (
                  <div className="relative flex gap-4" key={item.id}>
                    {showLine ? (
                      <div className="absolute bottom-0 left-4 top-10 w-px bg-[color:var(--app-outline-variant)]/10" />
                    ) : null}
                    {item.actorAvatarUrl ? (
                      <img
                        alt={item.actorName}
                        className="z-10 h-8 w-8 rounded-full border border-[var(--app-outline-variant)]/30 object-cover"
                        src={item.actorAvatarUrl}
                      />
                    ) : (
                      <div className="z-10 inline-flex h-8 w-8 items-center justify-center rounded-full border border-[var(--app-outline-variant)]/30 bg-[var(--app-surface-container-high)] text-[10px] font-bold text-[var(--app-outline)]">
                        {getDashboardInitials(item.actorName)}
                      </div>
                    )}
                    <div className="min-w-0 flex-1 space-y-1">
                      <p className="text-sm text-[var(--app-on-surface)]">
                        <span className="font-bold">{item.actorName}</span> {item.action} <span className="app-shell-mono text-xs text-[var(--app-primary)]">{item.ticketLabel}</span>
                      </p>
                      <p className="text-xs italic text-[var(--app-on-surface-variant)]">&quot;{item.detail}&quot;</p>
                      <span className="app-shell-mono text-[10px] text-[var(--app-outline)]">{item.timestamp}</span>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </section>
    </RouteLayout>
  );
}
