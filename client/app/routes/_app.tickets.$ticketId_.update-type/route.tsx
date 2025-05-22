import { ActionFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ActionResponse, ForbiddenResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import updateTicketType from "./server.update-type";

export async function action({ request, params }: ActionFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);
    const userRole = session.get("user").role
    const formData = await request.formData();
    const type = formData.get("type") as string;

    if (!validateRole(userRole, permissions.ticket.create)) {
        return ForbiddenResponse()
    }

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(
        updateTicketType(
            ticketId,
            type,
            tokenResponse.token));

    if (error) {
        return ActionResponse({
            success: false,
            error: error.message,
            headers: tokenResponse.headers
        })
    }

    return ActionResponse({
        success: true,
        error: null,
        headers: tokenResponse.headers
    })
}
