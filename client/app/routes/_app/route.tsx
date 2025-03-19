import { Link, Outlet, redirect, useLocation, useNavigation } from "@remix-run/react";
import SideMenu from "./SideMenu";
import { useEffect, useMemo } from "react";
import DashboardNavbar from "./DashboardNavbar";
import { filterMenuRoutesByRoles, menuRoutes, createRouteNameMap } from "~/data/routes";

const routeNameMap = createRouteNameMap(menuRoutes);

export async function loader() {
    return null
}

export default function AppRoute() {
    useEffect(() => {
        document.querySelector("body")?.classList.contains("overflow-hidden") || document.querySelector("body")?.classList.add("overflow-hidden")
    }, [])

    return (
        <div>
            <div className="flex min-h-screen ">
                {/* sidebar */}
                <SideMenu menuRoutes={filterMenuRoutesByRoles(menuRoutes, ["admin"])} />

                {/* contnet */}
                <div className="w-full">
                    <DashboardNavbar />
                    <div className="overflow-auto h-[calc(100vh-64px)]">
                        <Outlet />
                    </div>
                </div>
            </div>
        </div >
    )
}