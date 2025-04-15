import { Link, Outlet, redirect, useLoaderData, useLocation, useNavigation } from "@remix-run/react";
import SideMenu from "./SideMenu";
import { useEffect, useMemo } from "react";
import DashboardNavbar from "./DashboardNavbar";
import { filterMenuRoutesByRoles, menuRoutes } from "~/data/routes";
import { HeadersFunction, LoaderFunctionArgs } from "@remix-run/node";
import { getSession } from "~/services/sessions.server";
import { UserInfoResponse } from "~/services/api.server/types";
import { JsonResponse, JsonResponseResult } from "~/utils/response";


export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const user = session.get("user")

    // Check if user is authenticated
    if (!user) {
        return redirect("/login");
    }

    if (!user.companyId) {
        return redirect("/setup");
    }

    return JsonResponse({ data: user, error: null });
}

export default function AppRoute() {
    const { data: userData, error } = useLoaderData<typeof loader>() as JsonResponseResult<UserInfoResponse>;

    return (
        <div>
            <div className="flex min-h-screen h-screen max-h-screen bg-base-300">
                {/* sidebar */}
                <SideMenu menuRoutes={filterMenuRoutesByRoles(menuRoutes, ["admin"])} />

                {/* contnet */}
                <div className="w-full">
                    <DashboardNavbar avatarUrl={userData!.avatarUrl} />
                    <div className="overflow-y-auto h-[calc(100vh-64px)]">
                        <Outlet context={userData!} />
                    </div>
                </div>
            </div>
        </div >
    )
}
