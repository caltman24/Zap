import { useNavigate, useSearchParams } from "@remix-run/react";
import type { KeyboardEvent } from "react";
import { useMemo } from "react";
import { BasicTicketInfo } from "~/services/api.server/types";
import { formatDateTimeShort } from "~/utils/dateTime";

export type TicketTableProps = {
    tickets?: BasicTicketInfo[] | null
    enableFiltering?: boolean
}

type SortBy = "updatedAt" | "title" | "status" | "priority" | "type";
type SortDirection = "asc" | "desc";

const SORT_BY_OPTIONS: SortBy[] = ["updatedAt", "title", "status", "priority", "type"];
const SORT_DIRECTION_OPTIONS: SortDirection[] = ["asc", "desc"];
const DEFAULT_SEARCH_QUERY = "";
const DEFAULT_FILTER = "all";
const DEFAULT_SORT_BY: SortBy = "updatedAt";
const DEFAULT_SORT_DIRECTION: SortDirection = "desc";

const PARAMS = {
    query: "ticketQuery",
    status: "ticketStatus",
    priority: "ticketPriority",
    type: "ticketType",
    sortBy: "ticketSortBy",
    sortDirection: "ticketSortDirection"
} as const;

const priorityOrder: Record<string, number> = {
    urgent: 4,
    high: 3,
    medium: 2,
    low: 1
};

function compareStrings(left: string, right: string): number {
    return left.localeCompare(right, undefined, { sensitivity: "base" });
}

function getTicketLastUpdated(ticket: BasicTicketInfo): number {
    return new Date(ticket.updatedAt ?? ticket.createdAt).getTime();
}

export default function TicketTable({ tickets, enableFiltering = true }: TicketTableProps) {
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();

    const searchQuery = searchParams.get(PARAMS.query) ?? DEFAULT_SEARCH_QUERY;
    const statusFilter = searchParams.get(PARAMS.status) ?? DEFAULT_FILTER;
    const priorityFilter = searchParams.get(PARAMS.priority) ?? DEFAULT_FILTER;
    const typeFilter = searchParams.get(PARAMS.type) ?? DEFAULT_FILTER;

    const sortByParam = searchParams.get(PARAMS.sortBy);
    const sortDirectionParam = searchParams.get(PARAMS.sortDirection);
    const sortBy = SORT_BY_OPTIONS.includes(sortByParam as SortBy) ? (sortByParam as SortBy) : DEFAULT_SORT_BY;
    const sortDirection = SORT_DIRECTION_OPTIONS.includes(sortDirectionParam as SortDirection)
        ? (sortDirectionParam as SortDirection)
        : DEFAULT_SORT_DIRECTION;

    const allTickets = useMemo(() => tickets ?? [], [tickets]);

    const statusOptions = useMemo(() => {
        return [...new Set(allTickets.map((ticket) => ticket.status))].sort(compareStrings);
    }, [allTickets]);

    const priorityOptions = useMemo(() => {
        return [...new Set(allTickets.map((ticket) => ticket.priority))].sort(compareStrings);
    }, [allTickets]);

    const typeOptions = useMemo(() => {
        return [...new Set(allTickets.map((ticket) => ticket.type))].sort(compareStrings);
    }, [allTickets]);

    function updateParam(key: string, value: string, defaultValue: string): void {
        setSearchParams((currentParams) => {
            const nextParams = new URLSearchParams(currentParams);
            if (value === defaultValue) {
                nextParams.delete(key);
            } else {
                nextParams.set(key, value);
            }
            return nextParams;
        }, { replace: true });
    }

    function clearFiltersAndSorting(): void {
        setSearchParams((currentParams) => {
            const nextParams = new URLSearchParams(currentParams);
            nextParams.delete(PARAMS.query);
            nextParams.delete(PARAMS.status);
            nextParams.delete(PARAMS.priority);
            nextParams.delete(PARAMS.type);
            nextParams.delete(PARAMS.sortBy);
            nextParams.delete(PARAMS.sortDirection);
            return nextParams;
        }, { replace: true });
    }

    const visibleTickets = useMemo(() => {
        const normalizedQuery = searchQuery.trim().toLowerCase();

        const filtered = allTickets.filter((ticket) => {
            if (enableFiltering) {
                if (statusFilter !== "all" && ticket.status !== statusFilter) {
                    return false;
                }

                if (priorityFilter !== "all" && ticket.priority !== priorityFilter) {
                    return false;
                }

                if (typeFilter !== "all" && ticket.type !== typeFilter) {
                    return false;
                }

                if (normalizedQuery.length > 0) {
                    const assigneeName = ticket.assignee?.name ?? "";
                    const searchableText = `${ticket.name} ${ticket.submitter.name} ${assigneeName}`.toLowerCase();
                    if (!searchableText.includes(normalizedQuery)) {
                        return false;
                    }
                }
            }

            return true;
        });

        return [...filtered].sort((left, right) => {
            let comparison = 0;

            switch (sortBy) {
                case "title":
                    comparison = compareStrings(left.name, right.name);
                    break;
                case "status":
                    comparison = compareStrings(left.status, right.status);
                    break;
                case "priority": {
                    const leftPriority = priorityOrder[left.priority.toLowerCase()] ?? 0;
                    const rightPriority = priorityOrder[right.priority.toLowerCase()] ?? 0;
                    comparison = leftPriority - rightPriority;
                    break;
                }
                case "type":
                    comparison = compareStrings(left.type, right.type);
                    break;
                case "updatedAt":
                default:
                    comparison = getTicketLastUpdated(left) - getTicketLastUpdated(right);
                    break;
            }

            return sortDirection === "asc" ? comparison : -comparison;
        });
    }, [allTickets, enableFiltering, priorityFilter, searchQuery, sortBy, sortDirection, statusFilter, typeFilter]);

    function getTicketRoute(ticket: BasicTicketInfo): string {
        return `/projects/${ticket.projectId}/tickets/${ticket.id}`;
    }

    function handleTicketRowKeyDown(event: KeyboardEvent<HTMLTableRowElement>, route: string): void {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            navigate(route);
        }
    }

    return (
        <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-3">
                {enableFiltering && (
                    <>
                        <input
                            type="text"
                            className="input input-bordered input-sm w-full sm:w-64"
                            placeholder="Search title, submitter, assignee"
                            value={searchQuery}
                            onChange={(event) => updateParam(PARAMS.query, event.target.value, DEFAULT_SEARCH_QUERY)}
                        />
                        <select
                            className="select select-bordered select-sm"
                            value={statusFilter}
                            onChange={(event) => updateParam(PARAMS.status, event.target.value, DEFAULT_FILTER)}
                        >
                            <option value="all">All Statuses</option>
                            {statusOptions.map((status) => (
                                <option key={status} value={status}>{status}</option>
                            ))}
                        </select>
                        <select
                            className="select select-bordered select-sm"
                            value={priorityFilter}
                            onChange={(event) => updateParam(PARAMS.priority, event.target.value, DEFAULT_FILTER)}
                        >
                            <option value="all">All Priorities</option>
                            {priorityOptions.map((priority) => (
                                <option key={priority} value={priority}>{priority}</option>
                            ))}
                        </select>
                        <select
                            className="select select-bordered select-sm"
                            value={typeFilter}
                            onChange={(event) => updateParam(PARAMS.type, event.target.value, DEFAULT_FILTER)}
                        >
                            <option value="all">All Types</option>
                            {typeOptions.map((type) => (
                                <option key={type} value={type}>{type}</option>
                            ))}
                        </select>
                    </>
                )}
                <select
                    className="select select-bordered select-sm"
                    value={sortBy}
                    onChange={(event) => updateParam(PARAMS.sortBy, event.target.value, DEFAULT_SORT_BY)}
                >
                    <option value="updatedAt">Sort: Last Updated</option>
                    <option value="title">Sort: Title</option>
                    <option value="status">Sort: Status</option>
                    <option value="priority">Sort: Priority</option>
                    <option value="type">Sort: Type</option>
                </select>
                <button
                    type="button"
                    className="btn btn-sm btn-outline"
                    onClick={() => updateParam(
                        PARAMS.sortDirection,
                        sortDirection === "asc" ? "desc" : "asc",
                        DEFAULT_SORT_DIRECTION
                    )}
                >
                    {sortDirection === "asc" ? "Ascending" : "Descending"}
                </button>
                <button
                    type="button"
                    className="btn btn-sm btn-ghost"
                    onClick={clearFiltersAndSorting}
                >
                    {enableFiltering ? "Reset Filters and Sorting" : "Reset Sorting"}
                </button>
            </div>

            <div className="overflow-x-auto">
                <table className="table table-zebra w-full">
                    <thead>
                        <tr>
                            <th>Title</th>
                            <th>Status</th>
                            <th>Priority</th>
                            <th>Type</th>
                            <th>Submitter</th>
                            <th>Developer</th>
                            <th>Last Updated</th>
                        </tr>
                    </thead>
                    <tbody>
                        {visibleTickets.length > 0 ? (
                            visibleTickets.map((ticket) => (
                                <tr
                                    key={ticket.id}
                                    className="cursor-pointer transition-opacity duration-150 hover:opacity-70 focus-within:opacity-70"
                                    tabIndex={0}
                                    onClick={() => navigate(getTicketRoute(ticket))}
                                    onKeyDown={(event) => handleTicketRowKeyDown(event, getTicketRoute(ticket))}
                                >
                                    <td>
                                        <span className="link link-hover">{ticket.name}</span>
                                    </td>
                                    <td>
                                        <div>
                                            {getStatusDisplay(ticket.status)}
                                        </div>
                                    </td>
                                    <td>
                                        <div>
                                            {getPriorityDisplay(ticket.priority)}
                                        </div>
                                    </td>
                                    <td>
                                        <span className="flex items-center gap-1">
                                            {getTypeDisplay(ticket.type)}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="flex items-center gap-2">
                                            <div className="avatar">
                                                <div className="w-6 rounded-full">
                                                    <img src={ticket.submitter.avatarUrl} alt={ticket.submitter.name} />
                                                </div>
                                            </div>
                                            <span className="text-xs">{ticket.submitter.name}</span>
                                        </div>
                                    </td>
                                    <td>
                                        {ticket.assignee ? (
                                            <div className="flex items-center gap-2">
                                                <div className="avatar">
                                                    <div className="w-6 rounded-full">
                                                        <img src={ticket.assignee.avatarUrl} alt={ticket.assignee.name} />
                                                    </div>
                                                </div>
                                                <span className="text-xs">{ticket.assignee.name}</span>
                                            </div>
                                        ) : (
                                            <span className="text-xs opacity-60">Unassigned</span>
                                        )}
                                    </td>
                                    <td>
                                        {ticket.updatedAt ? (
                                            <span className="text-xs">
                                                {formatDateTimeShort(new Date(ticket.updatedAt))}
                                            </span>
                                        ) : (
                                            <span className="text-xs">
                                                {formatDateTimeShort(new Date(ticket.createdAt))}
                                            </span>
                                        )}
                                    </td>
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan={7} className="text-center py-4">
                                    No tickets found
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    )
}

// Helper function to get priority display with emoji
function getPriorityDisplay(priority: string): string {
    switch (priority?.toLowerCase()) {
        case 'urgent':
            return '🔴 Urgent';
        case 'high':
            return '🟠 High';
        case 'medium':
            return '🟡 Medium';
        case 'low':
            return '🟢 Low';
        default:
            return priority;
    }
}

// Helper function to get status display with emoji
function getStatusDisplay(status: string): string {
    switch (status?.toLowerCase()) {
        case 'new':
            return '🆕 New';
        case 'in development':
            return '⚙️ In Development';
        case 'testing':
            return '🧪 Testing';
        case 'resolved':
            return '✅ Resolved';
        case 'open':
            return '🆕 Open';
        case 'in progress':
            return '⚙️ In Progress';
        case 'closed':
            return '🔒 Closed';
        default:
            return status;
    }
}

// Helper function to get type display with emoji
function getTypeDisplay(type: string): string {
    switch (type?.toLowerCase()) {
        case 'defect':
            return '🐛 Defect';
        case 'feature':
            return '✨ Feature';
        case 'general task':
            return '📋 General Task';
        case 'work task':
            return '💼 Work Task';
        case 'change request':
            return '🔄 Change Request';
        case 'enhancement':
            return '⚡ Enhancement';
        default:
            return type;
    }
}
