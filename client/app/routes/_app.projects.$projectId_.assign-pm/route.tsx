import {ActionFunctionArgs, redirect} from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import {UserInfoResponse} from "~/services/api.server/types";
import {getSession} from "~/services/sessions.server";
import {ActionResponse, ForbiddenResponse} from "~/utils/response";
import {hasPermission} from "~/utils/permissions";
import tryCatch from "~/utils/tryCatch";
import {assignPM} from "./server.assign-pm";

// Assign pm to project
export async function action({request, params}: ActionFunctionArgs) {
    const projectId = params.projectId!
    const session = await getSession(request);
    const user = session.get("user") as UserInfoResponse;
    const formData = await request.formData();
    const memberId = formData.get("memberId");

    if (!hasPermission(user.permissions, "project.assignPm")) {
        return ForbiddenResponse()
    }

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const {data: res, error} = await tryCatch(assignPM(
        projectId,
        memberId as string,
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
