import { data, LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { JsonResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { getProjectDevList } from "./server.get-dev-list";

// get project managers that isnt assigned to the project
export async function loader({ request, params }: LoaderFunctionArgs) {
    const ticketId = params.ticketId!
    const session = await getSession(request);

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    try {
        const devListPromise = await getProjectDevList(
            ticketId,
            tokenResponse.token);

        return JsonResponse({
            data: devListPromise,
            error: null,
            headers: tokenResponse.headers
        })
    } catch (e: any) {
        return JsonResponse({
            data: null,
            error: e,
            headers: tokenResponse.headers
        })
    }
}
