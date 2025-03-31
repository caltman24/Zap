import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link } from "@remix-run/react";
import { getRolesByRouteName } from "~/data/routes";
import { getSession } from "~/services/sessions.server";

export const handle = {
    breadcrumb: () => <Link to="/projects/new">New</Link>,
};

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const user = session.get("user");
    const allowedRoles = getRolesByRouteName("Create Project");

    if (!user) {
        return redirect("/logout");
    }

    if (!allowedRoles.includes(user.role.toLowerCase())) {
        throw Response.json("Unauthorized", { status: 401, statusText: "Unauthorized" });
    }

    return Response.json({ user });
}

export default function NewProjectRoute() {
    return (
        <div className="text-center w-full bg-base-300 h-full p-6">
            <h1 className="text-3xl font-bold">Create New Project</h1>
        </div>
    );
}
