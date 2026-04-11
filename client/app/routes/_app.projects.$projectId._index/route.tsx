import {ActionFunctionArgs} from "@remix-run/node";
import {useOutletContext} from "@remix-run/react";
import ProjectCommonRoute from "~/commonRoutes/projectDetails/commonRoute"
import projectAction from "~/commonRoutes/projectDetails/server.action"
import {JsonResponseResult} from "~/utils/response";
import {ProjectResponse, UserInfoResponse} from "~/services/api.server/types";


export default function ProjectDetailsRoute() {
    const {loaderData, userInfo} = useOutletContext<{
        loaderData: JsonResponseResult<ProjectResponse>,
        userInfo: UserInfoResponse
    }>();
    return <ProjectCommonRoute loaderData={loaderData} userInfo={userInfo}/>
}

export async function action(actionParams: ActionFunctionArgs) {
    return projectAction(actionParams);
}
