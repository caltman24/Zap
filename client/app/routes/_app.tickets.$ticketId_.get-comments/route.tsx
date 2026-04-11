import {data, LoaderFunctionArgs, redirect} from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import {getSession} from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";
import {getTicketComments} from "./server.get-comments";

// get project managers that isnt assigned to the project
export async function loader({request, params}: LoaderFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    try {
        const ticketComments = await getTicketComments(
            ticketId,
            tokenResponse.token);

        return data({
            data: ticketComments,
            error: null,
            headers: tokenResponse.headers
        })
    } catch (e: any) {
        return data({
            data: null,
            error: e,
            headers: tokenResponse.headers
        })
    }
}
