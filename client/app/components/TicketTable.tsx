import { Form, Link } from "@remix-run/react";
import { BasicTicketInfo } from "~/services/api.server/types";

export type TicketTableProps = {
    tickets?: BasicTicketInfo[] | null
}

export default function TicketTable({ tickets }: TicketTableProps) {
    return (
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
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {tickets?.length! > 0 ? (
                        tickets?.map((ticket) => (
                            <tr key={ticket.id}>
                                <td>{ticket.name}</td>
                                <td>
                                    <div className={`badge ${getStatusClass(ticket.status)}`}>
                                        {ticket.status}
                                    </div>
                                </td>
                                <td>
                                    <div className={`badge ${getPriorityClass(ticket.priority)}`}>
                                        {ticket.priority}
                                    </div>
                                </td>
                                <td>{ticket.type}</td>
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
                                    <div className="flex gap-1 items-center">
                                        {/* FIXME: Add confirmation modal before delete */}
                                        {/* //TODO: This needs to NOT redirect */}
                                        <Form method="post" action={`/tickets/${ticket.id}/delete`} navigate={false} >
                                            <button type="submit" className="btn btn-error btn-xs">D</button>
                                        </Form>
                                        <Link to={`/projects/${ticket.projectId}/tickets/${ticket.id}`} className="btn btn-xs btn-ghost">
                                            <span className="material-symbols-outlined">visibility</span>
                                        </Link>
                                    </div>
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
        case 'open':
            return 'badge-info';
        case 'in progress':
            return 'badge-warning';
        case 'resolved':
            return 'badge-success';
        case 'closed':
            return 'badge-neutral';
        default:
            return 'badge-ghost';
    }
}
