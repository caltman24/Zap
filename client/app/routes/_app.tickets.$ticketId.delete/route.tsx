import { ActionFunctionArgs, data, LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ActionResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import removeTicket from "./server.delete-ticket";

export async function action({ request, params }: ActionFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);
    const formData = await request.formData();
    const projectId = formData.get("projectId") as string;

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(removeTicket(
        ticketId,
        tokenResponse.token
    ))

    if (error) {
        return ActionResponse({
            success: false,
            error: error.message,
            headers: tokenResponse.headers
        })
    }

    return redirect(`/projects/${projectId}`);
}
