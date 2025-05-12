import RouteLayout from "~/layouts/RouteLayout";
import { Link, useLoaderData } from "@remix-run/react";
import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { getSession } from "~/services/sessions.server";
import apiClient from "~/services/api.server/apiClient";
import tryCatch from "~/utils/tryCatch";
import { AuthenticationError } from "~/services/api.server/errors";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import { CompanyProjectsResponse } from "~/services/api.server/types";
import ProjectCard from "~/components/ProjectCard";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(
        apiClient.getCompanyArchivedProjects(tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return JsonResponse({
            data: null,
            error: error.message,
            headers: tokenResponse.headers
        })
    }

    return JsonResponse({
        data: res,
        error: null,
        headers: tokenResponse.headers
    });
}

export default function ArchivedProjectsRoute() {
    const { data, error } = useLoaderData<typeof loader>() as JsonResponseResult<CompanyProjectsResponse[]>;
    return (
        <RouteLayout>
            <h1 className="text-3xl font-bold mb-6">Archived Projects</h1>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {data?.map((project, index) => (
                    <ProjectCard key={index} project={project} collection="archived" />
                ))}
            </div>
        </RouteLayout>
    );
}
