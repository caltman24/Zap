import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { UserInfoResponse } from "~/services/api.server/types";
import { destroySession, getSession } from "~/services/sessions.server";
import { ActionResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import toggleArchiveTicket from "./server.archive-ticket";

export async function action({ request, params }: ActionFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);
    const formData = await request.formData();
    const intent = formData.get("intent") as "archive" | "unarchive";
    const projectId = formData.get("projectId") as string

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(
        toggleArchiveTicket(
            ticketId,
            tokenResponse.token));

    if (error) {
        return ActionResponse({
            success: false,
            error: error.message,
            headers: tokenResponse.headers
        })
    }

    // if (intent === "archive") {
    //     return redirect(`/tickets/archived/${ticketId}`)
    // }

    return redirect(`/projects/${projectId}/tickets/${ticketId}`)
}
