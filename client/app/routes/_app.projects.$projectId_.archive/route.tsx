import {ActionFunctionArgs, redirect} from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import {getSession} from "~/services/sessions.server";
import {ActionResponse} from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export async function action({request, params}: ActionFunctionArgs) {
    const projectId = params.projectId!
    const session = await getSession(request);
    const formData = await request.formData();
    const intent = formData.get("intent") as "archive" | "unarchive";

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const {data: res, error} = await tryCatch(
        apiClient.toggleArchiveProject(
            projectId,
            tokenResponse.token));

    if (error) {
        return ActionResponse({
            success: false,
            error: error.message,
            headers: tokenResponse.headers
        })
    }

    if (intent === "archive") {
        return redirect(`/projects/archived/${projectId}`)
    }

    return redirect(`/projects/${projectId}`)
}
