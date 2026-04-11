import RouteLayout from "~/layouts/RouteLayout";
import {type LoaderFunctionArgs} from "@remix-run/node";
import {Link, redirect, useLoaderData, useOutletContext} from "@remix-run/react";
import ProjectCard from "~/components/ProjectCard";
import apiClient from "~/services/api.server/apiClient";
import {AuthenticationError} from "~/services/api.server/errors";
import type {CompanyProjectsResponse, UserInfoResponse} from "~/services/api.server/types";
import {getSession} from "~/services/sessions.server";
import {hasPermission} from "~/utils/permissions";
import {JsonResponse, type JsonResponseResult} from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export async function loader({request}: LoaderFunctionArgs) {
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

    const {data: res, error} = await tryCatch(
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
    const {data, error} = useLoaderData<JsonResponseResult<CompanyProjectsResponse[]>>();
    const userInfo = useOutletContext<UserInfoResponse>();
    const totalProjects = data?.length ?? 0;

    if (error) {
        return (
            <RouteLayout>
                <div
                    className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
                    {error}
                </div>
            </RouteLayout>
        );
    }

    return (
        <RouteLayout className="space-y-6">
            <div className="flex flex-wrap items-end justify-between gap-4">
                <div className="space-y-2">
                    <div>
                        <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">All
                            Projects</h1>
                        <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">
                            All active projects in your company.
                        </p>
                    </div>
                </div>

                <div className="flex flex-wrap items-center gap-3">
                    <span
                        className="app-shell-mono text-xs uppercase tracking-[0.22em] text-[var(--app-outline)]">{totalProjects} active</span>

                    {hasPermission(userInfo.permissions, "project.create") ? (
                        <Link
                            className="inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2 text-xs font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95"
                            to="/projects/new"
                        >
                            <span className="material-symbols-outlined text-sm">add</span>
                            New Project
                        </Link>
                    ) : null}
                </div>
            </div>

            {totalProjects > 0 ? (
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
                    {data?.map((project) => (
                        <ProjectCard collection="projects" key={project.id} project={project} showArchived={false}/>
                    ))}
                </div>
            ) : (
                <div
                    className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-8 text-center outline outline-1 outline-[var(--app-outline-variant-soft)]">
                    <p className="text-base text-[var(--app-on-surface-variant)]">No projects match your current
                        scope.</p>
                </div>
            )}
        </RouteLayout>
    );
}
