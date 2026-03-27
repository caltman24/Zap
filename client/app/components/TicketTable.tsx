import { useNavigate } from "@remix-run/react";
import { useMemo, useState, type KeyboardEvent } from "react";
import type { BasicTicketInfo } from "~/services/api.server/types";
import useTicketListFiltering from "~/hooks/useTicketListFiltering";
import useTicketSearchState, { DEFAULT_TICKET_SEARCH_QUERY } from "~/hooks/useTicketSearchState";
import TicketAssignee from "./TicketAssignee";
import TicketSearchInput from "./TicketSearchInput";
import TicketSelectControl from "./TicketSelectControl";
import TicketTitlePreview from "./TicketTitlePreview";
import { formatDateTimeShort } from "~/utils/dateTime";
import {
  compareTicketStrings,
  getTicketPriorityDotClass,
  getTicketStatusChipClass,
} from "./ticketTableUtils";

export type TicketTableProps = {
  tickets?: BasicTicketInfo[] | null;
  enableFiltering?: boolean;
};

type SortBy = "updatedAt" | "title" | "status" | "priority" | "type";
type SortDirection = "asc" | "desc";

const DEFAULT_FILTER = "all";
const DEFAULT_SORT_BY: SortBy = "updatedAt";
const DEFAULT_SORT_DIRECTION: SortDirection = "desc";

const priorityOrder: Record<string, number> = {
  urgent: 4,
  high: 3,
  medium: 2,
  low: 1,
};

function getTicketLastUpdated(ticket: BasicTicketInfo): number {
  return new Date(ticket.updatedAt ?? ticket.createdAt).getTime();
}

export default function TicketTable({ tickets, enableFiltering = true }: TicketTableProps) {
  const navigate = useNavigate();
  const { searchQuery, setSearchQuery, normalizedSearchQuery } = useTicketSearchState();
  const [statusFilter, setStatusFilter] = useState(DEFAULT_FILTER);
  const [priorityFilter, setPriorityFilter] = useState(DEFAULT_FILTER);
  const [typeFilter, setTypeFilter] = useState(DEFAULT_FILTER);
  const [sortBy, setSortBy] = useState<SortBy>(DEFAULT_SORT_BY);
  const [sortDirection, setSortDirection] = useState<SortDirection>(DEFAULT_SORT_DIRECTION);

  const allTickets = useMemo(() => tickets ?? [], [tickets]);

  const statusOptions = useMemo(() => [...new Set(allTickets.map((ticket) => ticket.status))].sort(compareTicketStrings), [allTickets]);
  const priorityOptions = useMemo(() => [...new Set(allTickets.map((ticket) => ticket.priority))].sort(compareTicketStrings), [allTickets]);
  const typeOptions = useMemo(() => [...new Set(allTickets.map((ticket) => ticket.type))].sort(compareTicketStrings), [allTickets]);

  const { filteredTickets } = useTicketListFiltering({
    enableFiltering,
    normalizedSearchQuery,
    priorityFilter,
    statusFilter,
    tickets: allTickets,
    typeFilter,
  });

  function clearFiltersAndSorting(): void {
    setSearchQuery(DEFAULT_TICKET_SEARCH_QUERY);
    setStatusFilter(DEFAULT_FILTER);
    setPriorityFilter(DEFAULT_FILTER);
    setTypeFilter(DEFAULT_FILTER);
    setSortBy(DEFAULT_SORT_BY);
    setSortDirection(DEFAULT_SORT_DIRECTION);
  }

  const visibleTickets = useMemo(() => {
    return [...filteredTickets].sort((left, right) => {
      let comparison = 0;

      switch (sortBy) {
        case "title":
          comparison = compareTicketStrings(left.name, right.name);
          break;
        case "status":
          comparison = compareTicketStrings(left.status, right.status);
          break;
        case "priority": {
          const leftPriority = priorityOrder[left.priority.toLowerCase()] ?? 0;
          const rightPriority = priorityOrder[right.priority.toLowerCase()] ?? 0;
          comparison = leftPriority - rightPriority;
          break;
        }
        case "type":
          comparison = compareTicketStrings(left.type, right.type);
          break;
        case "updatedAt":
        default:
          comparison = getTicketLastUpdated(left) - getTicketLastUpdated(right);
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [filteredTickets, sortBy, sortDirection]);

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
    <div className="overflow-hidden rounded-[1.75rem] bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
      <div className="flex flex-wrap items-center gap-3 bg-[color:var(--app-surface-container-high)]/50 p-4">
        {enableFiltering ? (
          <TicketSearchInput
            className="min-w-[15rem] flex-1"
            onChange={setSearchQuery}
            placeholder="Search title, type, assignee"
            value={searchQuery}
          />
        ) : null}

        {enableFiltering ? (
          <>
            <TicketSelectControl
              className="min-w-[10rem]"
              onChange={(event) => setStatusFilter(event.target.value)}
              value={statusFilter}
            >
              <option value="all">All Statuses</option>
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </TicketSelectControl>

            <TicketSelectControl
              className="min-w-[10rem]"
              onChange={(event) => setPriorityFilter(event.target.value)}
              value={priorityFilter}
            >
              <option value="all">All Priorities</option>
              {priorityOptions.map((priority) => (
                <option key={priority} value={priority}>
                  {priority}
                </option>
              ))}
            </TicketSelectControl>

            <TicketSelectControl
              className="min-w-[10rem]"
              onChange={(event) => setTypeFilter(event.target.value)}
              value={typeFilter}
            >
              <option value="all">All Types</option>
              {typeOptions.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </TicketSelectControl>
          </>
        ) : null}

        <TicketSelectControl
          className="min-w-[11rem]"
          onChange={(event) => setSortBy(event.target.value as SortBy)}
          value={sortBy}
        >
          <option value="updatedAt">Sort: Last Updated</option>
          <option value="title">Sort: Title</option>
          <option value="status">Sort: Status</option>
          <option value="priority">Sort: Priority</option>
          <option value="type">Sort: Type</option>
        </TicketSelectControl>

        <button
          className="inline-flex h-11 cursor-pointer items-center gap-2 rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] px-4 text-sm text-[var(--app-on-surface)] transition-colors hover:bg-[var(--app-surface-container-high)]"
          onClick={() => setSortDirection((current) => (current === "asc" ? "desc" : "asc"))}
          type="button"
        >
          <span className="material-symbols-outlined text-lg">
            {sortDirection === "asc" ? "arrow_upward" : "arrow_downward"}
          </span>
          {sortDirection === "asc" ? "Ascending" : "Descending"}
        </button>

        <button
          className="inline-flex h-11 cursor-pointer items-center rounded-xl px-3.5 text-sm text-[var(--app-outline)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
          onClick={clearFiltersAndSorting}
          type="button"
        >
          {enableFiltering ? "Reset Filters" : "Reset Sorting"}
        </button>
      </div>

      <div className="divide-y divide-[color:var(--app-outline-variant)]/5 md:hidden">
        {visibleTickets.length > 0 ? (
          visibleTickets.map((ticket) => {
            const route = getTicketRoute(ticket);

            return (
              <button
                className="flex w-full flex-col gap-4 p-6 text-left transition-colors hover:bg-[color:var(--app-surface-container-highest)]/25"
                key={ticket.id}
                onClick={() => navigate(route)}
                type="button"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0 space-y-2">
                    <TicketTitlePreview
                      descriptionClassName="text-xs text-[var(--app-on-surface-variant)]"
                      descriptionLength={92}
                      ticket={ticket}
                      titleClassName="text-lg font-semibold text-[var(--app-on-surface)]"
                    />
                  </div>
                  <span className={`app-shell-mono inline-flex shrink-0 rounded-md px-2.5 py-1 text-[10px] uppercase tracking-[0.2em] ${getTicketStatusChipClass(ticket.status)}`}>
                    {ticket.status}
                  </span>
                </div>

                <div className="flex flex-wrap items-center gap-4 text-sm text-[var(--app-on-surface)]">
                  <div className="flex items-center gap-2">
                    <span className={`h-2.5 w-2.5 rounded-full ${getTicketPriorityDotClass(ticket.priority)}`} />
                    <span>{ticket.priority}</span>
                  </div>
                  <span className="app-shell-mono text-xs text-[var(--app-outline)]">
                    {formatDateTimeShort(new Date(ticket.updatedAt ?? ticket.createdAt))}
                  </span>
                </div>

                <TicketAssignee fallbackLabel="Unassigned" person={ticket.assignee} size="md" />
              </button>
            );
          })
        ) : (
          <div className="px-6 py-14 text-center text-sm text-[var(--app-on-surface-variant)]">
            No tickets found for the current filters.
          </div>
        )}
      </div>

      <div className="hidden overflow-x-auto md:block">
        <table className="w-full min-w-[1050px] border-collapse text-left">
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
            {visibleTickets.length > 0 ? (
              visibleTickets.map((ticket) => {
                const route = getTicketRoute(ticket);

                return (
                  <tr
                    className="group cursor-pointer transition-colors duration-150 hover:bg-[color:var(--app-surface-container-highest)]/35 focus-visible:outline-none focus-visible:bg-[color:var(--app-surface-container-highest)]/35"
                    key={ticket.id}
                    onClick={() => navigate(route)}
                    onKeyDown={(event) => handleTicketRowKeyDown(event, route)}
                    tabIndex={0}
                  >
                    <td className="px-6 py-5 align-top">
                      <TicketTitlePreview
                        descriptionClassName="max-w-[32rem] truncate text-xs text-[var(--app-on-surface-variant)]"
                        ticket={ticket}
                        titleClassName="text-[1rem] font-semibold text-[var(--app-on-surface)] transition-colors group-hover:text-[var(--app-primary)]"
                      />
                    </td>
                    <td className="px-6 py-5 align-top">
                      <span className={`app-shell-mono inline-flex rounded-md px-2.5 py-1 text-[10px] uppercase tracking-[0.2em] ${getTicketStatusChipClass(ticket.status)}`}>
                        {ticket.status}
                      </span>
                    </td>
                    <td className="px-6 py-5 align-top">
                      <div className="flex items-center gap-2 text-sm text-[var(--app-on-surface)]">
                        <span className={`h-2.5 w-2.5 rounded-full ${getTicketPriorityDotClass(ticket.priority)}`} />
                        <span>{ticket.priority}</span>
                      </div>
                    </td>
                    <td className="min-w-[13rem] px-6 py-5 align-top">
                      <TicketAssignee fallbackLabel="Unassigned" person={ticket.assignee} />
                    </td>
                    <td className="px-6 py-5 align-top">
                      <span className="app-shell-mono text-xs text-[var(--app-outline)]">
                        {formatDateTimeShort(new Date(ticket.updatedAt ?? ticket.createdAt))}
                      </span>
                    </td>
                  </tr>
                );
              })
            ) : (
              <tr>
                <td className="px-6 py-14 text-center text-sm text-[var(--app-on-surface-variant)]" colSpan={5}>
                  No tickets found for the current filters.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3 bg-[var(--app-surface-container-lowest)] px-4 py-4">
        <span className="app-shell-mono text-xs text-[var(--app-outline)]">
          Showing {visibleTickets.length} of {allTickets.length} tickets
        </span>
        <span className="text-xs text-[var(--app-on-surface-variant)]">
          Select any row to open the full ticket details.
        </span>
      </div>
    </div>
  );
}
