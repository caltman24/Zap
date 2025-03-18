import { Outlet } from "@remix-run/react";
import MainNavbar from "./MainNavbar";

export default function LandingRoute() {
    return (
        <div>
            <MainNavbar isAuthenticated={true} />
            <Outlet />
        </div>
    )
}