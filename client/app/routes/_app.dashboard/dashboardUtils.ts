import roleNames, { RoleName } from "~/data/roles";
import {
  BasicTicketInfo,
  CompanyProjectsResponse,
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
  recentActivity: BasicTicketInfo[];
  upcomingDeadlines: DashboardDeadline[];
};

export function getPriorityBadgeClass(priority: string): string {
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

export function getStatusBadgeClass(status: string): string {
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

export function getTypeBadgeClass(type: string): string {
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
    return `${minutes} minute${minutes === 1 ? "" : "s"} ago`;
  }

  if (diffMs < day) {
    const hours = Math.floor(diffMs / hour);
    return `${hours} hour${hours === 1 ? "" : "s"} ago`;
  }

  const days = Math.floor(diffMs / day);
  if (days < 7) return `${days} day${days === 1 ? "" : "s"} ago`;

  return new Date(isoDate).toLocaleDateString();
}

export function formatDeadlineDays(daysRemaining: number): string {
  if (daysRemaining < 0) {
    const overdueDays = Math.abs(daysRemaining);
    return `Overdue by ${overdueDays} day${overdueDays === 1 ? "" : "s"}`;
  }

  if (daysRemaining === 0) return "Due today";

  return `${daysRemaining} day${daysRemaining === 1 ? "" : "s"} remaining`;
}

export function toUpcomingDeadlines(
  projects: CompanyProjectsResponse[]
): DashboardDeadline[] {
  const now = new Date();
  const todayStart = new Date(
    now.getFullYear(),
    now.getMonth(),
    now.getDate()
  ).getTime();

  return projects
    .map((project) => {
      const dueDateValue = new Date(project.dueDate);
      const dueDateStart = new Date(
        dueDateValue.getFullYear(),
        dueDateValue.getMonth(),
        dueDateValue.getDate()
      ).getTime();

      const daysRemaining = Math.floor(
        (dueDateStart - todayStart) / (24 * 60 * 60 * 1000)
      );

      return {
        id: project.id,
        name: project.name,
        priority: project.priority,
        dueDate: project.dueDate,
        daysRemaining,
      };
    })
    .sort(
      (left, right) =>
        new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime()
    )
    .slice(0, 5);
}

export function getDashboardProjectLabel(
  role: UserInfoResponse["role"]
): string {
  if (role === roleNames.admin) return "Total Projects";
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
    ticketLabel: role === roleNames.admin ? "Total Tickets" : "Visible Tickets",
    openTicketLabel: "Open Tickets",
    closedTicketLabel: "Closed Tickets",
  };
}

export function getDashboardDeadlineLabel(
  role: UserInfoResponse["role"]
): string {
  return role === roleNames.admin
    ? "Upcoming Deadlines"
    : "Upcoming Deadlines In Your Scope";
}

export function getDashboardSummaryTickets(
  role: UserInfoResponse["role"],
  memberId: string | undefined,
  tickets: BasicTicketInfo[] | null
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

export function getDashboardDescription(
  role: UserInfoResponse["role"]
): string {
  let description = "";

  switch (role) {
    case "admin":
      description = "Overview of projects and tickets in your company.";
      break;
    case roleNames.projectManager:
      description =
        "Overview of projects and tickets in your managed projects.";
      break;
    case "submitter":
      description =
        "Overview of tickets you have submitted within your assigned projects.";
      break;
    case "developer":
      description =
        "Overview of projects and tickets you have been assigned to.";
      break;
  }

  return description;
}
