import { Link, Outlet, redirect, useLoaderData, useLocation, useNavigation } from "@remix-run/react";
import SideMenu from "./SideMenu";
import { useEffect, useMemo } from "react";
import DashboardNavbar from "./DashboardNavbar";
import { filterMenuRoutesByRoles, menuRoutes } from "~/data/routes";
import { HeadersFunction, LoaderFunctionArgs } from "@remix-run/node";
import { commitSession, destroySession, getSession } from "~/services/sessions.server";
import { UserInfoResponse } from "~/services/api.server/types";
import { JsonResponse, JsonResponseResult } from "~/utils/response";
import apiClient from "~/services/api.server/apiClient";


export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const accessToken = session.get("tokens").accessToken
    console.log(accessToken)

    // INFO: Fetch new user info every page load, in case member data changed e.g member role
    const newUserInfo = await apiClient.getUserInfo(accessToken)
    if (!newUserInfo) {
        return redirect("/login", {
            headers: {
                "Set-Cookie": await destroySession(session),
            },
        });
    }
    if (!newUserInfo.companyId) {
        return redirect("/setup", {
            headers: {
                "Set-Cookie": await commitSession(session),
            }
        });
    }

    session.set("user", newUserInfo)
    return JsonResponse({
        data: newUserInfo,
        error: null,
        headers: {
            "Set-Cookie": await commitSession(session),
        },
    });
}

export default function AppRoute() {
    const { data: userData, error } = useLoaderData<typeof loader>() as JsonResponseResult<UserInfoResponse>;
    const navigation = useNavigation()

    return (
        <div>
            <div className="flex min-h-screen h-screen max-h-screen bg-base-300">
                {/* sidebar */}
                <SideMenu menuRoutes={filterMenuRoutesByRoles(menuRoutes, ["admin"])} />

                {/* contnet */}
                <div className="w-full">
                    <DashboardNavbar avatarUrl={userData!.avatarUrl} />
                    <div className="overflow-y-auto h-[calc(100vh-64px)]">
                        {navigation.state === "loading"
                            ? (<div className="w-full h-full grid place-items-center">
                                <div className="loading loading-dots loading-xl"></div>
                            </div>)
                            : <Outlet context={userData!} />}

                    </div>
                </div>
            </div>
        </div >
    )
}
