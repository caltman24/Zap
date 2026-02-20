import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, useLoaderData, useNavigate } from "@remix-run/react";
import type { KeyboardEvent } from "react";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { BasicTicketInfo, CompanyProjectsResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { requestJson } from "~/utils/api";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

type DashboardDeadline = {
    id: string;
    name: string;
    priority: string;
    dueDate: string;
    daysRemaining: number;
};

type DashboardData = {
    totalProjects: number;
    totalTickets: number;
    openTickets: number;
    closedTickets: number;
    recentActivity: BasicTicketInfo[];
    upcomingDeadlines: DashboardDeadline[];
};

function getPriorityBadgeClass(priority: string): string {
    switch (priority.toLowerCase()) {
        case "urgent":
            return "badge-error";
        case "high":
            return "badge-warning";
        case "medium":
            return "badge-info";
        case "low":
            return "badge-success";
        default:
            return "badge-neutral";
    }
}

function getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
        case "resolved":
        case "closed":
            return "badge-success";
        case "testing":
            return "badge-info";
        case "in development":
        case "in progress":
            return "badge-warning";
        case "new":
        case "open":
            return "badge-neutral";
        default:
            return "badge-ghost";
    }
}

function getTypeBadgeClass(type: string): string {
    switch (type.toLowerCase()) {
        case "defect":
            return "badge-error";
        case "enhancement":
        case "feature":
            return "badge-accent";
        case "change request":
            return "badge-info";
        default:
            return "badge-neutral";
    }
}

function getTicketUpdatedAt(ticket: BasicTicketInfo): number {
    return new Date(ticket.updatedAt ?? ticket.createdAt).getTime();
}

function formatRelativeTime(isoDate: string): string {
    const now = Date.now();
    const timestamp = new Date(isoDate).getTime();
    const diffMs = now - timestamp;
    const minute = 60 * 1000;
    const hour = 60 * minute;
    const day = 24 * hour;

    if (diffMs < minute) {
        return "Just now";
    }

    if (diffMs < hour) {
        const minutes = Math.floor(diffMs / minute);
        return `${minutes} minute${minutes === 1 ? "" : "s"} ago`;
    }

    if (diffMs < day) {
        const hours = Math.floor(diffMs / hour);
        return `${hours} hour${hours === 1 ? "" : "s"} ago`;
    }

    const days = Math.floor(diffMs / day);
    if (days < 7) {
        return `${days} day${days === 1 ? "" : "s"} ago`;
    }

    return new Date(isoDate).toLocaleDateString();
}

function formatDeadlineDays(daysRemaining: number): string {
    if (daysRemaining < 0) {
        const overdueDays = Math.abs(daysRemaining);
        return `Overdue by ${overdueDays} day${overdueDays === 1 ? "" : "s"}`;
    }

    if (daysRemaining === 0) {
        return "Due today";
    }

    return `${daysRemaining} day${daysRemaining === 1 ? "" : "s"} remaining`;
}

function toUpcomingDeadlines(projects: CompanyProjectsResponse[]): DashboardDeadline[] {
    const now = new Date();
    const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();

    return projects
        .map((project) => {
            const dueDateValue = new Date(project.dueDate);
            const dueDateStart = new Date(
                dueDateValue.getFullYear(),
                dueDateValue.getMonth(),
                dueDateValue.getDate()
            ).getTime();

            const daysRemaining = Math.floor((dueDateStart - todayStart) / (24 * 60 * 60 * 1000));

            return {
                id: project.id,
                name: project.name,
                priority: project.priority,
                dueDate: project.dueDate,
                daysRemaining
            };
        })
        .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
        .slice(0, 5);
}

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    try {
        const [projects, openTickets, resolvedTickets] = await Promise.all([
            apiClient.getCompanyProjects(tokenResponse.token),
            requestJson<BasicTicketInfo[]>("/tickets/open", { method: "GET" }, tokenResponse.token),
            requestJson<BasicTicketInfo[]>("/tickets/resolved", { method: "GET" }, tokenResponse.token)
        ]);

        const recentActivity = [...openTickets, ...resolvedTickets]
            .sort((left, right) => getTicketUpdatedAt(right) - getTicketUpdatedAt(left))
            .slice(0, 5);

        const dashboardData: DashboardData = {
            totalProjects: projects.length,
            totalTickets: openTickets.length + resolvedTickets.length,
            openTickets: openTickets.length,
            closedTickets: resolvedTickets.length,
            recentActivity,
            upcomingDeadlines: toUpcomingDeadlines(projects)
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
    breadcrumb: () => <Link to="/dashboard">Dashboard</Link>
};

export default function DashboardRoute() {
    const { data, error } = useLoaderData<JsonResponseResult<DashboardData>>();
    const navigate = useNavigate();

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
            <div className="grid grid-cols-1 gap-6 mb-8 md:grid-cols-4">
                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-primary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    </div>
                    <div className="stat-title">Total Projects</div>
                    <div className="stat-value text-primary">{data.totalProjects}</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-secondary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"></path></svg>
                    </div>
                    <div className="stat-title">Total Tickets</div>
                    <div className="stat-value text-secondary">{data.totalTickets}</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-accent">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path></svg>
                    </div>
                    <div className="stat-title">Open Tickets</div>
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
                    <div className="stat-title">Closed Tickets</div>
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
                                    className="cursor-pointer transition-colors duration-150 hover:bg-base-200 hover:shadow-sm focus-within:bg-base-200 focus-within:shadow-sm"
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

            <div className="grid grid-cols-1 gap-8 mb-8 lg:grid-cols-2">
                <div className="bg-base-100 p-6 rounded-box shadow">
                    <h2 className="text-xl font-semibold mb-4">Tickets Over Time</h2>
                    <div className="h-64 w-full bg-base-200 flex items-center justify-center">
                        <p className="text-base-content/60">Chart Visualization Placeholder</p>
                    </div>
                </div>

                <div className="bg-base-100 p-6 rounded-box shadow">
                    <h2 className="text-xl font-semibold mb-4">Tickets By Priority</h2>
                    <div className="h-64 w-full bg-base-200 flex items-center justify-center">
                        <p className="text-base-content/60">Chart Visualization Placeholder</p>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 gap-8 lg:grid-cols-5">
                <div className="bg-base-100 p-6 rounded-box shadow lg:col-span-3">
                    <h2 className="text-xl font-semibold mb-8">Upcoming Deadlines</h2>
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
                <div className="bg-base-100 p-6 rounded-box shadow lg:col-span-2">
                    <h2 className="text-xl font-semibold mb-4">Projects By Priority</h2>
                    <div className="h-64 w-full bg-base-200 flex items-center justify-center">
                        <p className="text-base-content/60">Chart Visualization Placeholder</p>
                    </div>
                </div>
            </div>
        </RouteLayout>
    );
}
