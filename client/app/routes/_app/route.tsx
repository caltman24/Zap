import { Link, Outlet, redirect, useLocation, useNavigation } from "@remix-run/react";
import SideMenu from "./SideMenu";
import { useEffect, useMemo } from "react";
import DashboardNavbar from "./DashboardNavbar";
import { filterMenuRoutesByRoles, menuRoutes, routeNameMap } from "~/data/routes";

export async function loader() {
    return null
}


export default function AppRoute() {
    const location = useLocation();

    const routeName = useMemo(() => {
        const path = location.pathname;
        let bestMatch = "";
        let bestMatchName = "Dashboard"

        Object.entries(routeNameMap).forEach(([key, value]) => {
            if (path.startsWith(key) && key.length > bestMatch.length) {
                bestMatch = key;
                bestMatchName = value;
            }
        })

        return bestMatchName;
    }, [location])

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
                    <DashboardNavbar routeName={routeName} />
                    <div className="overflow-auto h-[calc(100vh-64px)]">
                        <Outlet />
                    </div>
                </div>
            </div>
        </div >
    )
}