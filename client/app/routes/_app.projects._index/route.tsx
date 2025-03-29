import { LoaderFunctionArgs } from "@remix-run/node";
import { Link, redirect, useLoaderData, useOutletContext } from "@remix-run/react";
import { useMemo } from "react";
import ProjectCard from "~/components/ProjectCard";
import { getRolesByRouteName } from "~/data/routes";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { CompanyProjectsResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

export const handle = {
    breadcrumb: () => <Link to="/projects">All Projects</Link>,
};

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const { data: res, error } = await tryCatch(apiClient.getCompanyProjects(tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return Response.json({ data: null, error: "Failed to get company projects. Please try again later." }, {
            headers: {
                ...tokenResponse.headers,
            }
        });
    }
    console.log(res)

    return Response.json({
        data: res,
        error: null
    }, {
        headers: {
            ...tokenResponse.headers,
        }
    });
}
export default function ProjectsRoute() {
    const { data, error } = useLoaderData<typeof loader>();
    const userInfo = useOutletContext<UserInfoResponse>();
    const createProjectRoles = useMemo(() => getRolesByRouteName("Create Project"), []);

    if (error) {
        return <p className="text-error">{error}</p>;
    }

    return (
        <div className="w-full bg-base-300 min-h-full p-6">
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-3xl font-bold">All Projects</h1>
                {createProjectRoles.includes(userInfo.role.toLowerCase()) && (
                    <Link to="/projects/new" className="btn btn-primary">
                        <span className="material-symbols-outlined mr-1">add_circle</span>
                        New Project
                    </Link>
                )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {(data as CompanyProjectsResponse[]).map((project, index) => (
                    <ProjectCard key={index} project={project} />
                ))}
            </div>
        </div>
    );
}

// Helper function to get badge color based on priority
function getPriorityClass(priority: string): string {
    switch (priority.toLowerCase()) {
        case 'high':
            return 'badge-error';
        case 'medium':
            return 'badge-warning';
        case 'low':
            return 'badge-info';
        default:
            return 'badge-ghost';
    }
}
