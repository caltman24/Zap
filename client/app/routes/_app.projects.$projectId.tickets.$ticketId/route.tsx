
import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useLoaderData, useNavigate, useParams } from "@remix-run/react"; import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient"; import { AuthenticationError } from "~/services/api.server/errors";
import { getSession } from "~/services/sessions.server"; import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { getTicketById } from "./server.get-ticket";
import BackButton from "~/components/BackButton";
export const handle = {
    breadcrumb: (match: any) => {
        const ticketId = match.params.ticketId; const ticketName = match.data?.data?.name || "Ticket Details";
        return <Link to={`/tickets/mytickets/${ticketId}`}>{ticketName}</Link>;
    },
};
export async function loader({ request, params }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const { ticketId } = params;
    const { data: tokenResponse,
        error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));
    if (tokenError) {
        return redirect("/logout");
    }
    try {
        const response = await getTicketById(ticketId!, tokenResponse.token);
        return JsonResponse({
            data: response,
            error: null, headers: tokenResponse.headers
        });
    } catch (error: any) {
        if (error instanceof AuthenticationError) {
            return redirect("/logout");
        }
        return JsonResponse({
            data: null,
            error: error.message, headers: tokenResponse.headers
        });
    }
}

export default function TicketDetailsRoute() {
    const { data: ticket, error } = useLoaderData<JsonResponseResult<any>>();
    const { ticketId } = useParams();
    if (error) {
        return <p className="text-error">{error}</p>;
    }
    return (
        <RouteLayout>
            {ticket ? (
                <>
                    <div className="flex justify-between items-center mb-6">

                        <h1 className="text-3xl font-bold">{ticket.name}</h1>
                        <div className="flex gap-2">
                            {/* FIXME: Add confirmation modal before delete */}
                            {/* TODO: This needs to redirect ONLY if the ticket is deleted from the ticket page */}
                            <Form method="post" action={`/tickets/${ticketId}/delete`} >
                                <input type="hidden" value={ticket.projectId} name="projectId"></input>
                                <button type="submit" className="btn btn-error btn-sm">Delete</button>
                            </Form>
                            <BackButton />
                        </div>
                    </div>
                    <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6">

                        <div className="mb-4">
                            <h2 className="text-xl font-bold mb-2">Description</h2>
                            <p className="whitespace-pre-wrap">{ticket.description}</p>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">

                            <div className="stat bg-base-200 rounded-lg">
                                <div className="stat-title mb-2">Submitter</div>
                                {ticket.submitter && (
                                    <div className="flex gap-2 items-center">

                                        <div className="avatar">
                                            <div className="w-9 rounded-full">

                                                <img src={ticket.submitter.avatarUrl} />
                                            </div>
                                        </div>
                                        <div className="stat-value text-lg font-bold">

                                            {ticket.submitter.name}
                                        </div>
                                    </div>
                                )}
                            </div>
                            <div className="stat bg-base-200 rounded-lg">
                                <div className="stat-title">Priority</div>
                                <div
                                    className={`stat-value text-lg ${getPriorityClass(
                                        ticket.priority
                                    )}`}
                                >
                                    {ticket.priority}
                                </div>
                            </div>
                            <div className="stat bg-base-200 rounded-lg">

                                <div className="stat-title">Status</div>
                                <div
                                    className={`stat-value text-lg ${getStatusClass(
                                        ticket.status
                                    )}`}
                                >
                                    {ticket.status}
                                </div>
                            </div>
                            <div className="stat bg-base-200 rounded-lg">

                                <div className="stat-title">Type</div>
                                <div className="stat-value text-lg font-bold">

                                    {ticket.type}
                                </div>
                            </div>
                        </div>
                        <div className="mt-6">

                            <h2 className="text-xl font-bold mb-2">Developer</h2>
                            {ticket.assignee ? (
                                <div className="flex gap-2 items-center">
                                    <div className="avatar">
                                        <div className="w-9 rounded-full">
                                            <img src={ticket.assignee.avatarUrl} />
                                        </div>
                                    </div>
                                    <div className="font-bold">{ticket.assignee.name} </div>
                                </div>
                            ) : (
                                <p className="font-medium text-gray-400">Unassigned</p>
                            )}
                        </div>
                    </div>
                    <div className="bg-base-100 rounded-lg shadow-lg p-6">
                        <h2 className="text-xl font-bold mb-4">Comments</h2>
                        <p className="text-base-content/60">Comments feature coming soon</p>
                    </div>
                </>
            ) : (
                <div className="flex justify-center items-center h-full">
                    <p>Loading ticket details...</p>
                </div>
            )}
        </RouteLayout>
    );
}
// Helper function to get badge color based on priority
function getPriorityClass(priority: string): string {
    switch (priority?.toLowerCase()) {
        case 'high': return 'text-error';
        case 'medium': return 'text-warning';
        case 'low': return 'text-info';
        default: return '';
    }
}
// Helper function to get badge color based on status
function getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
        case 'open': return 'text-info';
        case 'in progress': return 'text-warning';
        case 'resolved': return 'text-success';
        case 'closed': return 'text-neutral';
        default: return '';
    }
}

























































































