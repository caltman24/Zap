import {data, LoaderFunctionArgs, redirect} from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import {UserInfoResponse} from "~/services/api.server/types";
import {getSession} from "~/services/sessions.server";
import {ForbiddenResponse} from "~/utils/response";
import {hasPermission} from "~/utils/permissions";
import tryCatch from "~/utils/tryCatch";
import {getAssignablePMs} from "./server.get-pms";

// get project managers that isnt assigned to the project
export async function loader({request, params}: LoaderFunctionArgs) {
    const projectId = params.projectId!
    const session = await getSession(request);
    const user = session.get("user") as UserInfoResponse;

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

    // Return promise to show skeleton
    try {
        const assignablePMsPromise = await getAssignablePMs(
            projectId,
            tokenResponse.token);

        return data({
            data: assignablePMsPromise,
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
