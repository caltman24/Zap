import RouteLayout from "~/layouts/RouteLayout";
import { Link, useLoaderData, useOutletContext } from "@remix-run/react";
import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { getSession } from "~/services/sessions.server";
import apiClient from "~/services/api.server/apiClient";
import tryCatch from "~/utils/tryCatch";
import { AuthenticationError } from "~/services/api.server/errors";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import { CompanyProjectsResponse, UserInfoResponse } from "~/services/api.server/types";
import ProjectCard from "~/components/ProjectCard";
import roleNames from "~/data/roles";
import { hasPermission } from "~/utils/permissions";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const userInfo = session.get("user") as UserInfoResponse;

    if (!hasPermission(userInfo.permissions, "project.viewArchived")) {
        return redirect("/projects/myprojects");
    }

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
    const userInfo = useOutletContext<UserInfoResponse>();
    const isProjectManager = userInfo.role.toLowerCase() === roleNames.projectManager;
    return (
        <RouteLayout>
            <div className="mb-6">
                <h1 className="text-3xl font-bold">{isProjectManager ? "Managed Archived Projects" : "Archived Projects"}</h1>
                <p className="text-base-content/65 mt-1">
                    {isProjectManager ? "Archived projects where you are the assigned project manager." : "Archived projects across your company."}
                </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {data?.map((project, index) => (
                    <ProjectCard key={index} project={project} collection="archived" />
                ))}
                {!data?.length && (
                    <p className="text-base-content/60">No archived projects match your current scope.</p>
                )}
            </div>
        </RouteLayout>
    );
}
