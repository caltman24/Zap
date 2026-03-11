import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, useLoaderData, useNavigate, useOutletContext } from "@remix-run/react";
import type { KeyboardEvent } from "react";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { BasicTicketInfo, CompanyProjectsResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { requestJson } from "~/utils/api";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import roleNames from "~/data/roles";
import { hasPermission } from "~/utils/permissions";
import getMyProjects from "../_app.projects.myprojects._index/server.get-myprojects";
import { getMyTickets } from "../_app.tickets.mytickets._index/server.get-mytickets";
import {
    DashboardData,
    formatDeadlineDays,
    formatRelativeTime,
    getDashboardDeadlineLabel,
    getDashboardProjectLabel,
    getDashboardSummaryTickets,
    getDashboardTicketLabels,
    getPriorityBadgeClass,
    getStatusBadgeClass,
    getTicketUpdatedAt,
    getTypeBadgeClass,
    toUpcomingDeadlines,
} from "./dashboardUtils";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const userInfo = session.get("user") as UserInfoResponse;
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

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
            ownTicketsRequest
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
            headers: tokenResponse.headers
        });
    } catch (error: unknown) {
        if (error instanceof AuthenticationError) {
            return redirect("/logout");
        }

        const errorMessage = error instanceof Error ? error.message : "Failed to load dashboard data.";

        return JsonResponse({
            data: null,
            error: errorMessage,
            headers: tokenResponse.headers
        });
    }
}

export const handle = {
    breadcrumb: () => <Link to="/dashboard">Dashboard</Link>,
    breadcrumbLabel: "Dashboard"
};

export default function DashboardRoute() {
    const { data, error } = useLoaderData<JsonResponseResult<DashboardData>>();
    const userInfo = useOutletContext<UserInfoResponse>();
    const navigate = useNavigate();
    const usesOwnTicketSummary = [roleNames.developer, roleNames.submitter].includes(userInfo.role);
    const projectLabel = getDashboardProjectLabel(userInfo.role);
    const { ticketLabel, openTicketLabel, closedTicketLabel } = getDashboardTicketLabels(userInfo.role);
    const deadlineLabel = getDashboardDeadlineLabel(userInfo.role);

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
                <p className="text-error">{error ?? "Unable to load dashboard data."}</p>
            </RouteLayout>
        );
    }

    return (
        <RouteLayout>
            <div className="mb-6">
                <h1 className="text-3xl font-bold">Dashboard</h1>
                <p className="text-base-content/65 mt-1">
                    Overview of projects and tickets currently visible to your role.
                </p>
            </div>
            <div className="grid grid-cols-1 gap-6 mb-8 md:grid-cols-4">
                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-primary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    </div>
                    <div className="stat-title">{projectLabel}</div>
                    <div className="stat-value text-primary">{data.totalProjects}</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-secondary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"></path></svg>
                    </div>
                    <div className="stat-title">{ticketLabel}</div>
                    <div className="stat-value text-secondary">{data.totalTickets}</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-accent">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path></svg>
                    </div>
                    <div className="stat-title">{openTicketLabel}</div>
                    <div className="stat-value text-accent">{data.openTickets}</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-content">
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            fill="none"
                            viewBox="0 0 24 24"
                            className="inline-block h-8 w-8 stroke-current">
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth="2"
                                d="M13 10V3L4 14h7v7l9-11h-7z"></path>
                        </svg>
                    </div>
                    <div className="stat-title">{closedTicketLabel}</div>
                    <div className="stat-value text-content">{data.closedTickets}</div>
                </div>
            </div>

            <div className="bg-base-100 rounded-box shadow mb-8">
                <h2 className="text-xl font-semibold p-6 pb-2">Recent Activity</h2>
                <div className="overflow-x-auto">
                    <table className="table table-zebra w-full 2xlxl:text-lg">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Status</th>
                                <th>Type</th>
                                <th>Priority</th>
                                <th>Assigned To</th>
                                <th>Last Updated</th>
                            </tr>
                        </thead>
                        <tbody>
                            {data.recentActivity.length > 0 ? data.recentActivity.map((ticket) => (
                                <tr
                                    key={ticket.id}
                                    className="cursor-pointer transition-opacity duration-150 hover:opacity-70 focus-within:opacity-70"
                                    tabIndex={0}
                                    onClick={() => navigate(getTicketRoute(ticket))}
                                    onKeyDown={(event) => handleTicketRowKeyDown(event, getTicketRoute(ticket))}
                                >
                                    <td className="font-medium">
                                        <span className="link link-hover">{ticket.name}</span>
                                    </td>
                                    <td>
                                        <div className={`badge ${getStatusBadgeClass(ticket.status)} h-auto w-max`}>
                                            {ticket.status}
                                        </div>
                                    </td>
                                    <td>
                                        <div className={`badge ${getTypeBadgeClass(ticket.type)} h-auto w-max`}>
                                            {ticket.type}
                                        </div>
                                    </td>
                                    <td>
                                        <div className={`badge ${getPriorityBadgeClass(ticket.priority)} h-auto w-max`}>
                                            {ticket.priority}
                                        </div>
                                    </td>
                                    <td>{ticket.assignee?.name ?? "Unassigned"}</td>
                                    <td>{formatRelativeTime(ticket.updatedAt ?? ticket.createdAt)}</td>
                                </tr>
                            )) : (
                                <tr>
                                    <td colSpan={6} className="text-center py-4">
                                        No recent ticket activity found.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            <div className="bg-base-100 p-6 rounded-box shadow mb-8">
                <h2 className="text-xl font-semibold mb-8">{deadlineLabel}</h2>
                <ul className="space-y-4">
                    {data.upcomingDeadlines.length > 0 ? data.upcomingDeadlines.map((deadline) => (
                        <li key={deadline.id} className="flex items-center gap-6">
                            <div className={`badge badge-lg ${getPriorityBadgeClass(deadline.priority)} w-28 whitespace-nowrap`}>
                                {new Date(deadline.dueDate).toLocaleDateString(undefined, { month: "short", day: "numeric" })}
                            </div>
                            <div className="flex w-full justify-between gap-4">
                                <h3 className="font-medium">{deadline.name}</h3>
                                <p className="text-sm opacity-70 whitespace-nowrap">{formatDeadlineDays(deadline.daysRemaining)}</p>
                            </div>
                        </li>
                    )) : (
                        <li className="text-base-content/60">No project deadlines found.</li>
                    )}
                </ul>
            </div>

        </RouteLayout>
    );
}
