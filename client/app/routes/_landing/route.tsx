import { Outlet, useLoaderData, useLocation } from "@remix-run/react";
import MainNavbar from "./MainNavbar";
import { getSession } from "~/services/sessions.server";
import { LoaderFunctionArgs } from "@remix-run/node";


export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    const isAuthenticated = session.get("user") || false;

    return Response.json({ isAuthenticated });
}

export default function LandingRoute() {
    const { isAuthenticated } = useLoaderData<typeof loader>();
    const location = useLocation();
    const isHomepage = location.pathname === "/";

    return (
        <div>
            {!isHomepage && <MainNavbar isAuthenticated={Boolean(isAuthenticated)} />}
            <Outlet context={{ isAuthenticated: Boolean(isAuthenticated) }} />
        </div>
    )
}
