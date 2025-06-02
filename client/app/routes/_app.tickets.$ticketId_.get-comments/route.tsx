import { data, LoaderFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ForbiddenResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import { getTicketComments } from "./server.get-comments";

// get project managers that isnt assigned to the project
export async function loader({ request, params }: LoaderFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);
    const userRole = session.get("user").role

    if (!validateRole(userRole, permissions.project.edit)) {
        return ForbiddenResponse()
    }

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
