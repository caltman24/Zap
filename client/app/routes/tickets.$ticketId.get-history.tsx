import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { getSession } from "~/services/sessions.server";
import { JsonResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import getTicketHistory from "~/routes/_app.projects.$projectId.tickets.$ticketId/server.get-ticket-history";

export async function loader({ request, params }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const { ticketId } = params;

    const { data: tokenResponse, error: tokenError } = await tryCatch(
        apiClient.auth.getValidToken(session)
    );

    if (tokenError) {
        return redirect("/logout");
    }

    try {
        const history = await getTicketHistory(ticketId!, tokenResponse.token);
        return JsonResponse({
            data: history,
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
