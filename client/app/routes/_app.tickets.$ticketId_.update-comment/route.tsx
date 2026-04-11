import {ActionFunctionArgs, redirect} from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import {getSession} from "~/services/sessions.server";
import {ActionResponse} from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import updateComment from "./server.update-comment";

export async function action({request, params}: ActionFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);
    const formData = await request.formData();
    const commentId = formData.get("commentId") as string;
    const message = formData.get("message") as string;

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const {data: res, error} = await tryCatch(updateComment(
        ticketId,
        commentId,
        message,
        tokenResponse.token
    ))

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
