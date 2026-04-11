import roleNames from "~/data/roles";
import type {
    BasicTicketInfo,
    CompanyProjectsResponse,
    RecentActivityInfo,
    UserInfoResponse,
} from "~/services/api.server/types";

export type DashboardDeadline = {
    id: string;
    name: string;
    priority: string;
    dueDate: string;
    daysRemaining: number;
};

export type DashboardData = {
    totalProjects: number;
    totalTickets: number;
    openTickets: number;
    closedTickets: number;
    recentTickets: BasicTicketInfo[];
    recentEvents: RecentActivityInfo[];
    upcomingDeadlines: DashboardDeadline[];
};

export type DashboardActivityItem = {
    id: string;
    actorName: string;
    actorAvatarUrl: string | null;
    action: string;
    ticketLabel: string;
    detail: string;
    timestamp: string;
    icon: string;
};

export function getStatusChipClass(status: string): string {
    switch (status.toLowerCase()) {
        case "resolved":
            return "bg-emerald-500/15 text-emerald-300";
        case "testing":
            return "bg-sky-500/20 text-sky-300";
        case "in development":
            return "bg-[var(--app-secondary-container)]/40 text-[var(--app-secondary)]";
        case "new":
            return "bg-[var(--app-surface-container-highest)] text-[var(--app-on-surface)]";
        default:
            return "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]";
    }
}

export function getPriorityDotClass(priority: string): string {
    switch (priority.toLowerCase()) {
        case "urgent":
        case "high":
            return "bg-[var(--app-error)]";
        case "medium":
            return "bg-[var(--app-tertiary)]";
        case "low":
            return "bg-[var(--app-outline)]";
        default:
            return "bg-[var(--app-secondary)]";
    }
}

export function getDeadlineTone(deadline: DashboardDeadline) {
    if (deadline.daysRemaining < 0) {
        return {
            containerClass: "bg-[var(--app-error-container)]/20 border-[var(--app-error)]/10",
            monthClass: "text-[var(--app-error)]",
            dayClass: "text-[var(--app-error)]",
            labelClass: "text-[var(--app-error)]",
        };
    }

    if (deadline.daysRemaining <= 3) {
        return {
            containerClass: "bg-[var(--app-tertiary-container)]/20 border-[var(--app-tertiary)]/10",
            monthClass: "text-[var(--app-tertiary)]",
            dayClass: "text-[var(--app-tertiary)]",
            labelClass: "text-[var(--app-tertiary)]",
        };
    }

    return {
        containerClass: "bg-[var(--app-surface-container-highest)]/20 border-white/5",
        monthClass: "text-[var(--app-outline)]",
        dayClass: "text-[var(--app-on-surface)]",
        labelClass: "text-[var(--app-outline)]",
    };
}

export function getTicketUpdatedAt(ticket: BasicTicketInfo): number {
    return new Date(ticket.updatedAt ?? ticket.createdAt).getTime();
}

export function formatRelativeTime(isoDate: string): string {
    const now = Date.now();
    const timestamp = new Date(isoDate).getTime();
    const diffMs = now - timestamp;
    const minute = 60 * 1000;
    const hour = 60 * minute;
    const day = 24 * hour;

    if (diffMs < minute) return "Just now";

    if (diffMs < hour) {
        const minutes = Math.floor(diffMs / minute);
        return `${minutes}m ago`;
    }

    if (diffMs < day) {
        const hours = Math.floor(diffMs / hour);
        return `${hours}h ago`;
    }

    const days = Math.floor(diffMs / day);
    if (days < 7) return `${days}d ago`;

    return new Date(isoDate).toLocaleDateString(undefined, {month: "short", day: "numeric"});
}

export function formatDeadlineDays(daysRemaining: number): string {
    if (daysRemaining < 0) {
        const overdueDays = Math.abs(daysRemaining);
        return overdueDays === 1 ? "Overdue" : `Overdue by ${overdueDays} days`;
    }

    if (daysRemaining === 0) return "Due today";
    if (daysRemaining <= 7) return `${daysRemaining} day${daysRemaining === 1 ? "" : "s"} left`;
    if (daysRemaining <= 14) return "Next week";

    return `${daysRemaining} days remaining`;
}

export function formatDashboardDateParts(isoDate: string) {
    const value = new Date(isoDate);

    return {
        month: value.toLocaleDateString(undefined, {month: "short"}).toUpperCase(),
        day: value.toLocaleDateString(undefined, {day: "2-digit"}),
    };
}

export function truncateDashboardText(text: string | null | undefined, maxLength = 68): string {
    if (!text) {
        return "No additional context yet for this issue.";
    }

    const normalized = text.trim().replace(/\s+/g, " ");

    if (normalized.length <= maxLength) {
        return normalized;
    }

    return `${normalized.slice(0, maxLength - 3)}...`;
}

export function getDashboardInitials(name: string): string {
    const parts = name
        .split(" ")
        .map((part) => part.trim())
        .filter(Boolean)
        .slice(0, 2);

    if (parts.length === 0) {
        return "NA";
    }

    return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
}

export function toDashboardActivityItems(
    events: RecentActivityInfo[],
    userInfo: UserInfoResponse,
): DashboardActivityItem[] {
    return events.map((event) => {
        const actorName = event.actor.id === userInfo.memberId ? "You" : event.actor.name;

        return {
            id: event.id,
            actorName,
            actorAvatarUrl: event.actor.avatarUrl ?? null,
            action: getDashboardActivityAction(event),
            ticketLabel: event.displayId,
            detail: getDashboardActivityDetail(event),
            timestamp: formatRelativeTime(event.occurredAt),
            icon: getDashboardActivityIcon(event),
        };
    });
}

function getDashboardActivityAction(event: RecentActivityInfo): string {
    switch (event.type) {
        case "ticketCreated":
            return "created";
        case "statusChanged":
            return "changed status on";
        case "priorityChanged":
            return "changed priority on";
        case "assigneeChanged":
            return "updated assignee on";
        case "commentAdded":
            return "commented on";
    }
}

function getDashboardActivityDetail(event: RecentActivityInfo): string {
    switch (event.type) {
        case "ticketCreated":
            return truncateDashboardText(event.ticketName, 84);
        case "statusChanged":
            return formatDashboardValueChange(event.oldValue, event.newValue, "Status");
        case "priorityChanged":
            return formatDashboardValueChange(event.oldValue, event.newValue, "Priority");
        case "assigneeChanged":
            return formatDashboardValueChange(event.oldValue, event.newValue, "Assignee");
        case "commentAdded":
            return truncateDashboardText(event.message, 84);
    }
}

function getDashboardActivityIcon(event: RecentActivityInfo): string {
    switch (event.type) {
        case "ticketCreated":
            return "add_circle";
        case "statusChanged":
            return "sync_alt";
        case "priorityChanged":
            return "flag";
        case "assigneeChanged":
            return "person_add";
        case "commentAdded":
            return "chat";
    }
}

function formatDashboardValueChange(
    oldValue: string | null,
    newValue: string | null,
    label: string,
): string {
    if (oldValue && newValue) {
        return `${label}: ${oldValue} -> ${newValue}`;
    }

    if (newValue) {
        return `${label}: ${newValue}`;
    }

    if (oldValue) {
        return `${label}: ${oldValue} removed`;
    }

    return `${label} updated.`;
}

export function toUpcomingDeadlines(
    projects: CompanyProjectsResponse[],
): DashboardDeadline[] {
    const now = new Date();
    const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();

    return projects
        .map((project) => {
            const dueDateValue = new Date(project.dueDate);
            const dueDateStart = new Date(
                dueDateValue.getFullYear(),
                dueDateValue.getMonth(),
                dueDateValue.getDate(),
            ).getTime();

            const daysRemaining = Math.floor((dueDateStart - todayStart) / (24 * 60 * 60 * 1000));

            return {
                id: project.id,
                name: project.name,
                priority: project.priority,
                dueDate: project.dueDate,
                daysRemaining,
            };
        })
        .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
        .slice(0, 5);
}

export function getDashboardProjectLabel(role: UserInfoResponse["role"]): string {
    if (role === roleNames.admin) return "Active Projects";
    if (role === roleNames.projectManager) return "Projects In Scope";
    return "Assigned Projects";
}

export function getDashboardTicketLabels(role: UserInfoResponse["role"]) {
    if (role === roleNames.developer) {
        return {
            ticketLabel: "Assigned Tickets",
            openTicketLabel: "Assigned Open Tickets",
            closedTicketLabel: "Assigned Closed Tickets",
        };
    }

    if (role === roleNames.submitter) {
        return {
            ticketLabel: "Submitted Tickets",
            openTicketLabel: "Submitted Open Tickets",
            closedTicketLabel: "Submitted Closed Tickets",
        };
    }

    return {
        ticketLabel: role === roleNames.admin ? "Open Issues" : "Visible Tickets",
        openTicketLabel: "Open Tickets",
        closedTicketLabel: "Closed Tickets",
    };
}

export function getDashboardDeadlineLabel(role: UserInfoResponse["role"]): string {
    return role === roleNames.admin ? "Deadlines" : "Deadlines In Scope";
}

export function getDashboardSummaryTickets(
    role: UserInfoResponse["role"],
    memberId: string | undefined,
    tickets: BasicTicketInfo[] | null,
) {
    if (!tickets) return null;

    if (role === roleNames.developer) {
        return tickets.filter((ticket) => ticket.assignee?.id === memberId);
    }

    if (role === roleNames.submitter) {
        return tickets.filter((ticket) => ticket.submitter.id === memberId);
    }

    return tickets;
}
