import { ActionFunctionArgs, LoaderFunctionArgs } from "@remix-run/node";
import { Link, useLoaderData, useOutletContext } from "@remix-run/react";
import ProjectCommonRoute from "~/commonRoutes/projectDetails/commonRoute"
import projectLoader from "~/commonRoutes/projectDetails/server.loader"
import projectAction from "~/commonRoutes/projectDetails/server.action"
import { UserInfoResponse } from "~/services/api.server/types";

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
    const loaderData = useLoaderData<typeof loader>();
    const userInfo = useOutletContext<UserInfoResponse>();
    return <ProjectCommonRoute loaderData={loaderData} userInfo={userInfo} collection="archived" />
}
export async function action(actionParams: ActionFunctionArgs) {
    return projectAction(actionParams);
}
