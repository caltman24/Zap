import { Link, useNavigate } from "@remix-run/react";
import { useEffect, useMemo, useState, type KeyboardEvent } from "react";
import roleNames, { type RoleName } from "~/data/roles";
import type { BasicTicketInfo } from "~/services/api.server/types";
import useTicketListFiltering from "~/hooks/useTicketListFiltering";
import useTicketSearchState from "~/hooks/useTicketSearchState";
import { formatDateTimeShort } from "~/utils/dateTime";
import TicketAssignee from "./TicketAssignee";
import TicketSearchInput from "./TicketSearchInput";
import TicketSelectControl from "./TicketSelectControl";
import TicketTitlePreview from "./TicketTitlePreview";
import {
  compareTicketStrings,
  getTicketPriorityDotClass,
  getTicketStatusChipClass,
} from "./ticketTableUtils";

type DashboardTicketTableProps = {
  tickets: BasicTicketInfo[];
  totalTickets: number;
  currentMemberId?: string;
  currentUserRole: RoleName;
};

export default function DashboardTicketTable({
  tickets,
  totalTickets,
  currentMemberId,
  currentUserRole,
}: DashboardTicketTableProps) {
  const navigate = useNavigate();
  const { searchQuery, setSearchQuery, normalizedSearchQuery } = useTicketSearchState();
  const [typeFilter, setTypeFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");
  const [priorityFilter, setPriorityFilter] = useState("all");
  const [assigneeScope, setAssigneeScope] = useState("all");
  const normalizedCurrentUserRole = currentUserRole.toLowerCase() as RoleName;
  const allowAssignedToMeFilter = normalizedCurrentUserRole === roleNames.developer;

  useEffect(() => {
    if (!allowAssignedToMeFilter && assigneeScope === "mine") {
      setAssigneeScope("all");
    }
  }, [allowAssignedToMeFilter, assigneeScope]);

  const typeOptions = useMemo(
    () => [...new Set(tickets.map((ticket) => ticket.type))].sort(compareTicketStrings),
    [tickets],
  );
  const statusOptions = useMemo(
    () => [...new Set(tickets.map((ticket) => ticket.status))].sort(compareTicketStrings),
    [tickets],
  );
  const priorityOptions = useMemo(
    () => [...new Set(tickets.map((ticket) => ticket.priority))].sort(compareTicketStrings),
    [tickets],
  );

  const { filteredTickets } = useTicketListFiltering({
    assigneeScope,
    currentMemberId,
    normalizedSearchQuery,
    priorityFilter,
    statusFilter,
    tickets,
    typeFilter,
  });

  const visibleTickets = useMemo(() => {
    return filteredTickets;
  }, [filteredTickets]);

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
    <section className="overflow-hidden rounded-[1.75rem] bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
      <div className="flex flex-col gap-4 bg-[color:var(--app-surface-container-high)]/50 p-4 md:flex-row md:items-center md:justify-between">
        <div className="flex flex-1 flex-wrap items-center gap-3">
          <TicketSearchInput
            className="min-w-[15rem] flex-1"
            onChange={setSearchQuery}
            placeholder="Search title, type, assignee"
            value={searchQuery}
          />

          <TicketSelectControl
            aria-label="Type: All"
            className="min-w-[10rem]"
            onChange={(event) => setTypeFilter(event.target.value)}
            value={typeFilter}
          >
            <option value="all">Type: All</option>
            {typeOptions.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </TicketSelectControl>

          <TicketSelectControl
            aria-label="Status: All"
            className="min-w-[10rem]"
            onChange={(event) => setStatusFilter(event.target.value)}
            value={statusFilter}
          >
            <option value="all">Status: All</option>
            {statusOptions.map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </TicketSelectControl>

          <TicketSelectControl
            aria-label="Assignee: All"
            className="min-w-[11rem]"
            onChange={(event) => setAssigneeScope(event.target.value)}
            value={assigneeScope}
          >
            <option value="all">Assignee: All</option>
            {allowAssignedToMeFilter ? <option value="mine">Assigned to Me</option> : null}
            <option value="assigned">Assigned</option>
            <option value="unassigned">Unassigned</option>
          </TicketSelectControl>

          <TicketSelectControl
            aria-label="Priority: All"
            className="min-w-[10rem]"
            onChange={(event) => setPriorityFilter(event.target.value)}
            value={priorityFilter}
          >
            <option value="all">Priority: All</option>
            {priorityOptions.map((priority) => (
              <option key={priority} value={priority}>
                {priority}
              </option>
            ))}
          </TicketSelectControl>
        </div>

        <Link
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2 text-xs font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95"
          to="/tickets/new"
        >
          <span className="material-symbols-outlined text-sm">add</span>
          New Issue
        </Link>
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

                <TicketAssignee person={ticket.assignee} size="sm" />
              </button>
            );
          })
        ) : (
          <div className="px-6 py-12 text-center text-sm text-[var(--app-on-surface-variant)]">
            No recent ticket activity found.
          </div>
        )}
      </div>

      <div className="hidden overflow-x-auto md:block">
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
            {visibleTickets.length > 0 ? (
              visibleTickets.map((ticket) => {
                const route = getTicketRoute(ticket);

                return (
                  <tr
                    className="group cursor-pointer transition-colors duration-150 hover:bg-[color:var(--app-surface-container-highest)]/40 focus-within:bg-[color:var(--app-surface-container-highest)]/40"
                    key={ticket.id}
                    onClick={() => navigate(route)}
                    onKeyDown={(event) => handleTicketRowKeyDown(event, route)}
                    tabIndex={0}
                  >
                    <td className="px-6 py-5 align-top">
                      <TicketTitlePreview
                        descriptionClassName="text-xs text-[var(--app-on-surface-variant)]"
                        descriptionLength={78}
                        ticket={ticket}
                        titleClassName="text-[1.05rem] font-semibold text-[var(--app-on-surface)] transition-colors group-hover:text-[var(--app-primary)]"
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
                    <td className="px-6 py-5 align-top">
                      <TicketAssignee person={ticket.assignee} size="sm" />
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
          Showing 1-{visibleTickets.length} of {totalTickets} tickets
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
  );
}
