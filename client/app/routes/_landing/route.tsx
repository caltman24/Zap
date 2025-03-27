import { Outlet, useLoaderData } from "@remix-run/react";
import MainNavbar from "./MainNavbar";
import { getSession } from "~/services/sessions.server";
import { HeadersFunction, LoaderFunctionArgs } from "@remix-run/node";


export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    const isAuthenticated = session.get("user") || false;

    return Response.json({ isAuthenticated });
}

export default function LandingRoute() {
    const { isAuthenticated } = useLoaderData<typeof loader>();

    return (
        <div>
            <MainNavbar isAuthenticated={isAuthenticated} />
            <Outlet />
        </div>
    )
}