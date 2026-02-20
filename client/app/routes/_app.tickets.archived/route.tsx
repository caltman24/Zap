import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, useLoaderData, useLocation } from "@remix-run/react";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { BasicTicketInfo } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import TicketTable from "~/components/TicketTable";
import { getArchivedTickets } from "./server.get-archived-tickets";

function ArchivedTicketsBreadcrumb() {
    const location = useLocation();

    return <Link to={{ pathname: "/tickets/archived", search: location.search }}>Archived</Link>;
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
        const response = await getArchivedTickets(tokenResponse.token);

        return JsonResponse({
            data: response,
            error: null,
            headers: tokenResponse.headers
        });
    } catch (error: any) {
        if (error instanceof AuthenticationError) {
            return redirect("/logout");
        }

        return JsonResponse({
            data: null,
            error: error.message,
            headers: tokenResponse.headers
        });
    }
}

export const handle = {
    breadcrumb: () => <ArchivedTicketsBreadcrumb />,
};

export default function ArchivedTicketsRoute() {
    const { data: tickets, error } = useLoaderData<JsonResponseResult<BasicTicketInfo[]>>();

    if (error) {
        return <p className="text-error">{error}</p>;
    }

    return (
        <RouteLayout>
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-3xl font-bold">Archived Tickets</h1>
            </div>

            <div className="bg-base-100 rounded-lg shadow-lg p-6">
                <TicketTable tickets={tickets} />
            </div>
        </RouteLayout>
    );
}
