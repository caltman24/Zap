import { data, LoaderFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ForbiddenResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import getUnassignedProjectMembers from "./server.getUnassignedMembers";

// get unassigned members
export async function loader({ request, params }: LoaderFunctionArgs) {
    const projectId = params.projectId!
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

    // Return promise to show skeleton
    try {
        const unassignedMembersPromise = await getUnassignedProjectMembers(
            projectId,
            tokenResponse.token);

        return data({
            data: unassignedMembersPromise,
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
