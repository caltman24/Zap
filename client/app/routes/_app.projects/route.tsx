import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, Outlet, useOutletContext } from "@remix-run/react";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";
// this route is only for breadcrumbs
export const handle = {
    breadcrumb: () => <Link to="/projects">Projects</Link>,
};


export default function ProjectsRootRoute() {
    const userInfo = useOutletContext<UserInfoResponse>();

    return <Outlet context={userInfo} />;
}
