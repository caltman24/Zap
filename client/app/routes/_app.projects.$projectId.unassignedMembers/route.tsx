import { data, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Await, useLoaderData, useNavigate } from "@remix-run/react";
import { Suspense, useEffect, useRef, useState } from "react";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";


export async function loader({ request, params }: LoaderFunctionArgs) {
    const projectId = params.projectId!

    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    // Return promise to show skeleton
    try {
        const unassignedMembersPromise = await apiClient.getUnassignedProjectMembers(
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

