import RouteLayout from "~/layouts/RouteLayout";
import { useLoaderData, useOutletContext } from "@remix-run/react";
import { type LoaderFunctionArgs, redirect } from "@remix-run/node";
import { getSession } from "~/services/sessions.server";
import apiClient from "~/services/api.server/apiClient";
import tryCatch from "~/utils/tryCatch";
import { AuthenticationError } from "~/services/api.server/errors";
import { JsonResponse, type JsonResponseResult } from "~/utils/response";
import type { CompanyProjectsResponse, UserInfoResponse } from "~/services/api.server/types";
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
  const totalProjects = data?.length ?? 0;

  if (error) {
    return (
      <RouteLayout>
        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
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
            <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">
              {isProjectManager ? "Managed Archived Projects" : "Archived Projects"}
            </h1>
            <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">
              {isProjectManager
                ? "Archived projects where you are the assigned project manager."
                : "Archived projects across your company."}
            </p>
          </div>
        </div>

        <span className="app-shell-mono text-xs uppercase tracking-[0.22em] text-[var(--app-outline)]">{totalProjects} archived</span>
      </div>

      {totalProjects > 0 ? (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
          {data?.map((project) => (
            <ProjectCard collection="archived" key={project.id} project={project} showArchived />
          ))}
        </div>
      ) : (
        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-8 text-center outline outline-1 outline-[var(--app-outline-variant-soft)]">
          <p className="text-base text-[var(--app-on-surface-variant)]">No archived projects match your current scope.</p>
        </div>
      )}
    </RouteLayout>
  );
}
