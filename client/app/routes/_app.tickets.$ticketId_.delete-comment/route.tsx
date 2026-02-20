import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { UserInfoResponse } from "~/services/api.server/types";
import { destroySession, getSession } from "~/services/sessions.server";
import { ActionResponse, ForbiddenResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import deleteComment from "./server.delete-comment";

export async function action({ request, params }: ActionFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);
    const userRole = session.get("user").role
    const formData = await request.formData()
    const commentId = formData.get("commentId") as string

    if (!validateRole(userRole, permissions.comment.deleteOwn)) {
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
        deleteComment(
            ticketId,
            commentId,
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
