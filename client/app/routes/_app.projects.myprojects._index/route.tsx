import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";
import ProjectCard from "~/components/ProjectCard";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { getSession } from "~/services/sessions.server";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import getMyProjects from "./server.get-myprojects";
import { CompanyProjectsResponse, UserInfoResponse } from "~/services/api.server/types";
import { validateRole } from "~/utils/validate";
import permissions from "~/data/permissions";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));
    const userInfo = session.get("user") as UserInfoResponse

    if (tokenError) {
        return redirect("/logout");
    }

    if (!validateRole(userInfo.role, permissions.project.myprojects)) {
        return redirect("/projects")
    }

    const { data: res, error } = await tryCatch(
        getMyProjects(userInfo.memberId!, tokenResponse.token));

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
export default function MyProjectsRoute() {
    const { data, error } = useLoaderData<typeof loader>() as JsonResponseResult<CompanyProjectsResponse[]>;
    return (
        <RouteLayout>
            <div className="mb-6">
                <h1 className="text-3xl font-bold">My Projects</h1>
                <p className="text-base-content/65 mt-1">Projects you are assigned to and can actively work in.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {data?.map((project, index) => (
                    <ProjectCard key={index} project={project} collection="myprojects" />
                ))}
                {!data?.length && (
                    <p className="text-base-content/60">You do not have any assigned projects yet.</p>
                )}
            </div>
        </RouteLayout>
    );
}
