import { LoaderFunctionArgs } from "@remix-run/node";
import { Outlet, redirect } from "@remix-run/react";
import { getSession } from "~/services/sessions.server";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);

    // if (!session.get("isAuthenticated")) {
    //     return redirect("/login");
    // }

    return null;
}

export default function SetupRootRoute() {
    return <Outlet />;
}