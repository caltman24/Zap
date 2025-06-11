import { Form, Link } from "@remix-run/react";
import { BasicTicketInfo } from "~/services/api.server/types";
import { formatDateTimeShort } from "~/utils/dateTime";

export type TicketTableProps = {
    tickets?: BasicTicketInfo[] | null
}

export default function TicketTable({ tickets }: TicketTableProps) {
    return (
        <div className="overflow-x-auto">
            <table className="table table-zebra w-full">
                <thead>
                    <tr>
                        <th className="w-10"></th>
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
                    {tickets?.length! > 0 ? (
                        tickets?.map((ticket) => (
                            <tr key={ticket.id}>
                                <td>
                                    <div className="flex gap-1 items-center">
                                        <Link to={`/projects/${ticket.projectId}/tickets/${ticket.id}`} className="btn btn-xs btn-ghost">
                                            <span className="material-symbols-outlined">visibility</span>
                                        </Link>
                                    </div>
                                </td>
                                <td>{ticket.name}</td>
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
                            <td colSpan={6} className="text-center py-4">
                                No tickets found
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    )
}

// Helper function to get badge color based on priority
function getPriorityClass(priority: string): string {
    switch (priority?.toLowerCase()) {
        case 'urgent':
            return 'badge-error';
        case 'high':
            return 'badge-error';
        case 'medium':
            return 'badge-warning';
        case 'low':
            return 'badge-info';
        default:
            return 'badge-ghost';
    }
}

// Helper function to get badge color based on status
function getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
        case 'new':
            return 'badge-info';
        case 'in development':
            return 'badge-warning';
        case 'testing':
            return 'badge-warning';
        case 'resolved':
            return 'badge-success';
        case 'open':
            return 'badge-info';
        case 'in progress':
            return 'badge-warning';
        case 'closed':
            return 'badge-neutral';
        default:
            return 'badge-ghost';
    }
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
