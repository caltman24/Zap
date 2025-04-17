import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { destroySession, getSession } from "~/services/sessions.server";
import { ActionResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export async function action({ request, params }: ActionFunctionArgs) {
    const projectId = params.projectId!

    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(
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

    return ActionResponse({
        success: true,
        error: null,
        headers: tokenResponse.headers
    })
}
export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    if (!session.get("user")) {
        return redirect("/login");
    } else {
        return redirect("/", {
            headers: {
                "Set-Cookie": await destroySession(session),
            },
        });
    }
}

export default function ArchiveProject() {
    return null;
}
