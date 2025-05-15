import { LoaderFunctionArgs } from "@remix-run/node";
import { Link, Outlet, useLoaderData, useOutletContext } from "@remix-run/react";
import projectLoader from "~/commonRoutes/projectDetails/server.loader"
import { UserInfoResponse } from "~/services/api.server/types";

export const handle = {
    breadcrumb: (match: any) => {
        const projectId = match.params.projectId;
        const projectName = match.data?.data?.name || "Project Details";
        return <Link to={`/projects/${projectId}`}>{projectName}</Link>;
    },
};

export async function loader(loaderParams: LoaderFunctionArgs) {
    return projectLoader(loaderParams);
}

export default function ProjectDetailsRoot() {
    const loaderData = useLoaderData<typeof loader>();
    // User info is already provided by the _app root route outlet context
    const userInfo = useOutletContext<UserInfoResponse>();
    // Provide the project data and user info to sub routes
    return <Outlet context={{ loaderData, userInfo }} />
}
