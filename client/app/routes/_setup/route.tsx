import { LoaderFunctionArgs } from "@remix-run/node";
import { Outlet, redirect, useLoaderData } from "@remix-run/react";
import apiService from "~/services/ApiService.server";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const user = session.get("user")

    if (!user) {
        return redirect("/login")
    }

    if (user.companyId) {
        return redirect("/dashboard")
    }



    return Response.json({ firstName: user.firstName })
}

export default function SetupRootRoute() {
    const loaderData = useLoaderData<typeof loader>();

    return <Outlet context={loaderData} />;
}