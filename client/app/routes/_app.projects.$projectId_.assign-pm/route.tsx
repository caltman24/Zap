import { ActionFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ActionResponse, ForbiddenResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import { assignPM } from "./server.assign-pm";

// Assign pm to project
export async function action({ request, params }: ActionFunctionArgs) {
    const projectId = params.projectId!
    const session = await getSession(request);
    const userRole = session.get("user").role
    const formData = await request.formData();
    const memberId = formData.get("memberId");

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

    const { data: res, error } = await tryCatch(assignPM(
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
