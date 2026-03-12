import RouteLayout from "~/layouts/RouteLayout";
import { LoaderFunctionArgs } from "@remix-run/node";
import { Link, redirect, useLoaderData, useOutletContext } from "@remix-run/react";
import ProjectCard from "~/components/ProjectCard";
import roleNames from "~/data/roles";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { CompanyProjectsResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { hasPermission } from "~/utils/permissions";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const userInfo = session.get("user") as UserInfoResponse;

    if (!hasPermission(userInfo.permissions, "project.viewAll")) {
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
        apiClient.getCompanyProjects(tokenResponse.token));

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
export default function ProjectsRoute() {
    const { data, error } = useLoaderData<JsonResponseResult<CompanyProjectsResponse[]>>()
    const userInfo = useOutletContext<UserInfoResponse>();

    if (error) {
        return <p className="text-error">{error}</p>;
    }

    return (
        <RouteLayout>
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-3xl font-bold">All Projects</h1>
                    <p className="text-base-content/65 mt-1">
                        All active projects in your company.
                    </p>
                </div>
                {hasPermission(userInfo.permissions, "project.create") && (
                    <Link to="/projects/new" className="btn btn-soft">
                        <span className="material-symbols-outlined text-success">add_circle</span>
                        New Project
                    </Link>
                )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {data?.map((project, index) => (
                    <ProjectCard
                        key={index}
                        project={project}
                        showArchived={false}
                        collection="projects" />
                ))}
                {!data?.length && (
                    <p className="text-base-content/60">No projects match your current scope.</p>
                )}
            </div>
        </RouteLayout>
    );
}
