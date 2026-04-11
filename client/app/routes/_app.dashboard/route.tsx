import {type LoaderFunctionArgs, redirect} from "@remix-run/node";
import {Link, useLoaderData, useLocation, useOutletContext} from "@remix-run/react";
import DashboardTicketTable from "~/components/DashboardTicketTable";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import {AuthenticationError} from "~/services/api.server/errors";
import type {BasicTicketInfo, RecentActivityInfo, UserInfoResponse} from "~/services/api.server/types";
import {getSession} from "~/services/sessions.server";
import {requestJson} from "~/utils/api";
import {JsonResponse, type JsonResponseResult} from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import roleNames from "~/data/roles";
import {hasPermission} from "~/utils/permissions";
import getMyProjects from "../_app.projects.myprojects._index/server.get-myprojects";
import {getMyTickets} from "../_app.tickets.mytickets._index/server.get-mytickets";
import {
    type DashboardData,
    formatDashboardDateParts,
    formatDeadlineDays,
    getDashboardDeadlineLabel,
    getDashboardInitials,
    getDashboardProjectLabel,
    getDashboardSummaryTickets,
    getDashboardTicketLabels,
    getDeadlineTone,
    getTicketUpdatedAt,
    toDashboardActivityItems,
    toUpcomingDeadlines,
} from "./dashboardUtils";

export async function loader({request}: LoaderFunctionArgs) {
    const session = await getSession(request);
    const userInfo = session.get("user") as UserInfoResponse;
    const {data: tokenResponse, error: tokenError} = await tryCatch(apiClient.auth.getValidToken(session));

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

        const [projects, openTickets, resolvedTickets, ownTickets, recentEvents] = await Promise.all([
            projectsRequest,
            requestJson<BasicTicketInfo[]>("/tickets/open", {method: "GET"}, tokenResponse.token),
            requestJson<BasicTicketInfo[]>("/tickets/resolved", {method: "GET"}, tokenResponse.token),
            ownTicketsRequest,
            requestJson<RecentActivityInfo[]>("/tickets/recent-activity", {method: "GET"}, tokenResponse.token),
        ]);

        const summaryTickets = getDashboardSummaryTickets(userInfo.role, userInfo.memberId, ownTickets);

        const ownOpenTickets = summaryTickets?.filter((ticket) => ticket.status.toLowerCase() !== "resolved") ?? [];
        const ownResolvedTickets = summaryTickets?.filter((ticket) => ticket.status.toLowerCase() === "resolved") ?? [];
        const upcomingDeadlines = toUpcomingDeadlines(projects);

        const ticketSource = useOwnTicketSummary ? summaryTickets ?? [] : [...openTickets, ...resolvedTickets];
        const recentTickets = ticketSource
            .sort((left, right) => getTicketUpdatedAt(right) - getTicketUpdatedAt(left))
            .slice(0, 5);

        const dashboardData: DashboardData = {
            totalProjects: projects.length,
            totalTickets: useOwnTicketSummary ? summaryTickets?.length ?? 0 : openTickets.length + resolvedTickets.length,
            openTickets: useOwnTicketSummary ? ownOpenTickets.length : openTickets.length,
            closedTickets: useOwnTicketSummary ? ownResolvedTickets.length : resolvedTickets.length,
            recentTickets,
            recentEvents,
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
    const {data, error} = useLoaderData<JsonResponseResult<DashboardData>>();
    const userInfo = useOutletContext<UserInfoResponse>();
    const userRole = userInfo.role.toLowerCase() as UserInfoResponse["role"];
    const projectLabel = getDashboardProjectLabel(userRole);
    const {ticketLabel, openTicketLabel, closedTicketLabel} = getDashboardTicketLabels(userInfo.role);
    const deadlineLabel = getDashboardDeadlineLabel(userRole);
    const activityItems = data ? toDashboardActivityItems(data.recentEvents, userInfo) : [];
    const location = useLocation();

    const statCards = data
        ? [
            {
                label: projectLabel,
                value: data.totalProjects,
                mono: false,
                accentClass: "border-l-[var(--app-primary-fixed-strong)]"
            },
            {label: ticketLabel, value: data.totalTickets, mono: false, accentClass: "border-l-[var(--app-secondary)]"},
            {
                label: openTicketLabel,
                value: data.openTickets,
                mono: false,
                accentClass: "border-l-[var(--app-tertiary)]"
            },
            {
                label: closedTicketLabel,
                value: data.closedTickets,
                mono: false,
                accentClass: "border-l-[var(--app-success)]"
            },
        ]
        : [];

    if (error || !data) {
        return (
            <RouteLayout>
                <div
                    className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline-1 outline-[var(--app-outline-variant-soft)]">
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
                            className={`rounded-2xl border-l-2 bg-[var(--app-surface-container-low)] p-5 outline-1 outline-[var(--app-outline-variant-soft)] ${card.accentClass}`}
                            key={card.label}
                        >
                            <div className="mb-6 flex items-center justify-between gap-3">
                                <span className="app-shell-mono text-xs text-[var(--app-outline)]">{card.label}</span>
                            </div>
                            <div
                                className={`text-[2.1rem] font-bold tracking-[-0.04em] text-[var(--app-on-surface)] ${card.mono ? "app-shell-mono" : ""}`}>
                                {card.value}
                            </div>
                        </article>
                    );
                })}
            </section>

            <DashboardTicketTable
                currentMemberId={userInfo.memberId}
                currentUserRole={userRole}
                tickets={data.recentTickets}
                totalTickets={data.totalTickets}
            />

            <section className="grid grid-cols-1 gap-8 lg:grid-cols-12">
                <div className="space-y-4 lg:col-span-5">
                    <h2 className="flex items-center gap-2 text-xl font-bold tracking-tight text-[var(--app-on-surface)]">
                        <span className="material-symbols-outlined text-[var(--app-secondary)]">event</span>
                        {deadlineLabel}
                    </h2>

                    <div
                        className="overflow-hidden rounded-2xl bg-[var(--app-surface-container-low)] outline-1 outline-[var(--app-outline-variant-soft)]">
                        <div className="divide-y divide-[color:var(--app-outline-variant)]/5">
                            {data.upcomingDeadlines.length > 0 ? (
                                data.upcomingDeadlines.map((deadline) => {
                                    const tone = getDeadlineTone(deadline);
                                    const {month, day} = formatDashboardDateParts(deadline.dueDate);

                                    return (
                                        <Link
                                            className="group flex w-full items-center justify-between gap-4 p-4 text-left transition-colors hover:bg-[var(--app-surface-container-high)]/30"
                                            key={deadline.id}
                                            to={`/projects/${deadline.id}`}
                                            state={{from: location.pathname}}
                                            type="button"
                                        >
                                            <div className="flex min-w-0 items-center gap-4">
                                                <div
                                                    className={`flex h-10 w-10 shrink-0 flex-col items-center justify-center rounded-lg border ${tone.containerClass}`}>
                                                    <span
                                                        className={`app-shell-mono text-[10px] uppercase ${tone.monthClass}`}>{month}</span>
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
                                            <span
                                                className="material-symbols-outlined text-lg text-[var(--app-outline)] transition-transform group-hover:translate-x-1">
                        chevron_right
                      </span>
                                        </Link>
                                    );
                                })
                            ) : (
                                <div className="p-6 text-sm text-[var(--app-on-surface-variant)]">No project deadlines
                                    found.</div>
                            )}
                        </div>
                    </div>
                </div>

                <div className="space-y-4 lg:col-span-7">
                    <h2 className="flex items-center gap-2 text-xl font-bold tracking-tight text-[var(--app-on-surface)]">
                        <span className="material-symbols-outlined text-[var(--app-primary)]">history_edu</span>
                        Recent Activity
                    </h2>

                    <div
                        className="rounded-2xl bg-[var(--app-surface-container-low)] p-6 outline-1 outline-[var(--app-outline-variant-soft)]">
                        <div className="space-y-6">
                            {activityItems.length > 0 ? activityItems.map((item, index) => {
                                const showLine = index < activityItems.length - 1;

                                return (
                                    <Link
                                        className="group relative flex gap-4 rounded-xl p-2 -m-2 transition-colors hover:bg-[var(--app-surface-container-high)]/30"
                                        key={item.id}
                                        state={{from: location.pathname}}
                                        to={`/projects/${data.recentEvents[index]?.projectId}/tickets/${data.recentEvents[index]?.ticketId}`}
                                    >
                                        {showLine ? (
                                            <div
                                                className="absolute bottom-0 left-4 top-10 w-px bg-[color:var(--app-outline-variant)]/10"/>
                                        ) : null}
                                        {item.actorAvatarUrl ? (
                                            <img
                                                alt={item.actorName}
                                                className="z-10 h-8 w-8 rounded-full border border-[var(--app-outline-variant)]/30 object-cover"
                                                src={item.actorAvatarUrl}
                                            />
                                        ) : (
                                            <div
                                                className="z-10 inline-flex h-8 w-8 items-center justify-center rounded-full border border-[var(--app-outline-variant)]/30 bg-[var(--app-surface-container-high)] text-[10px] font-bold text-[var(--app-outline)]">
                                                {getDashboardInitials(item.actorName)}
                                            </div>
                                        )}
                                        <div className="min-w-0 flex-1 space-y-1">
                                            <p className="text-sm text-[var(--app-on-surface)]">
                                                <span className="font-bold">{item.actorName}</span> {item.action} <span
                                                className="app-shell-mono text-xs text-[var(--app-primary)]">{item.ticketLabel}</span>
                                            </p>
                                            <div
                                                className="flex items-center gap-2 text-xs text-[var(--app-on-surface-variant)]">
                                                <span
                                                    className="material-symbols-outlined text-sm text-[var(--app-outline)]">{item.icon}</span>
                                                <p className="min-w-0 italic">&quot;{item.detail}&quot;</p>
                                            </div>
                                            <span
                                                className="app-shell-mono text-[10px] text-[var(--app-outline)]">{item.timestamp}</span>
                                        </div>
                                    </Link>
                                );
                            }) : (
                                <div className="text-sm text-[var(--app-on-surface-variant)]">No recent activity
                                    found.</div>
                            )}
                        </div>
                    </div>
                </div>
            </section>
        </RouteLayout>
    );
}
