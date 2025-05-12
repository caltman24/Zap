import { ActionFunctionArgs, LoaderFunctionArgs } from "@remix-run/node";
import { Link } from "@remix-run/react";
import ProjectCommonRoute from "~/commonRoutes/projectDetails/commonRoute"
import projectLoader from "~/commonRoutes/projectDetails/server.loader"
import projectAction from "~/commonRoutes/projectDetails/server.action"

export const handle = {
    breadcrumb: (match: any) => {
        const projectId = match.params.projectId;
        const projectName = match.data?.data?.name || "Project Details";
        return <Link to={`/projects/archived/${projectId}`}>{projectName}</Link>;
    },
};

export async function loader(loaderParams: LoaderFunctionArgs) {
    return projectLoader(loaderParams);
}

export default function ArchivedProjectDetailsRoute() {
    return <ProjectCommonRoute />
}
export async function action(actionParams: ActionFunctionArgs) {
    return projectAction(actionParams);
}
