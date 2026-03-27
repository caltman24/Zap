import { useMemo } from "react";
import type { BasicTicketInfo } from "~/services/api.server/types";

type UseTicketListFilteringArgs = {
  tickets: BasicTicketInfo[];
  normalizedSearchQuery: string;
  typeFilter?: string;
  statusFilter?: string;
  priorityFilter?: string;
  assigneeScope?: string;
  currentMemberId?: string;
  enableFiltering?: boolean;
};

const DEFAULT_FILTER = "all";

function matchesTicketSearch(ticket: BasicTicketInfo, normalizedSearchQuery: string) {
  if (normalizedSearchQuery.length === 0) {
    return true;
  }

  const assigneeName = ticket.assignee?.name ?? "";
  const searchableText = `${ticket.name} ${ticket.description} ${ticket.type} ${assigneeName}`.toLowerCase();

  return searchableText.includes(normalizedSearchQuery);
}

export default function useTicketListFiltering({
  tickets,
  normalizedSearchQuery,
  typeFilter = DEFAULT_FILTER,
  statusFilter = DEFAULT_FILTER,
  priorityFilter = DEFAULT_FILTER,
  assigneeScope = DEFAULT_FILTER,
  currentMemberId,
  enableFiltering = true,
}: UseTicketListFilteringArgs) {
  const filteredTickets = useMemo(() => {
    if (!enableFiltering) {
      return tickets;
    }

    return tickets.filter((ticket) => {
      if (statusFilter !== DEFAULT_FILTER && ticket.status !== statusFilter) {
        return false;
      }

      if (priorityFilter !== DEFAULT_FILTER && ticket.priority !== priorityFilter) {
        return false;
      }

      if (typeFilter !== DEFAULT_FILTER && ticket.type !== typeFilter) {
        return false;
      }

      if (assigneeScope === "mine" && ticket.assignee?.id !== currentMemberId) {
        return false;
      }

      if (assigneeScope === "assigned" && !ticket.assignee) {
        return false;
      }

      if (assigneeScope === "unassigned" && ticket.assignee) {
        return false;
      }

      return matchesTicketSearch(ticket, normalizedSearchQuery);
    });
  }, [assigneeScope, currentMemberId, enableFiltering, normalizedSearchQuery, tickets, priorityFilter, statusFilter, typeFilter]);

  return {
    filteredTickets,
  };
}
